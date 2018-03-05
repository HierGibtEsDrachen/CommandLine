using CommandLine;
using CommandLine.Initializer;
using CommandLine.Reactor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Console
{
    [ContentProperty(nameof(Classes))]
    public class TestClass : InitializerObject
    {
        public IEnumerable<TestClass> Classes { get { return ((Dictionary<string, TestClass>) classesproperty.GetValue()).Values; } }
        public TestClass Class { get { return (TestClass) classproperty.GetValue(); } }
        public string Text { get { return (string) textproperty.GetValue(); } }
        public int Number { get { return (int) numberproperty.GetValue(); } }
        private readonly InitializerProperty numberproperty;
        private readonly InitializerProperty textproperty;
        private readonly InitializerProperty classproperty;
        private readonly InitializerProperty classesproperty;
        public TestClass()
        {
            numberproperty = Register(nameof(Number), typeof(int), new PropertyMetadata(0));
            textproperty = Register(nameof(Text), typeof(string), new PropertyMetadata(null));
            classproperty = Register(nameof(Class), typeof(TestClass), new PropertyMetadata(null));
            classesproperty = Register(nameof(Classes), typeof(Dictionary<string, TestClass>), 
                new PropertyMetadata(new Dictionary<string, TestClass>()));
        }
    }

    internal class Service1 : Service
    {
        public Service1(string nameSpace, string name) : base(nameSpace, name)
        {
        }
        protected override void Execute(ServiceState state)
        {
            System.Console.WriteLine("Service 1 executed");
        }
    }
    internal class Service2 : Service
    {
        public Service2(string nameSpace, string name) : base(nameSpace, name)
        {
            Register("check1", 'a', false, false, (s, a) => System.Console.WriteLine("Service 2 passed Check1 with: " + a));
            Register("check2", 'b', true, false, (s, a) => System.Console.WriteLine("Service 2 passed Check2 with: " + a));
            Register("check3", 'c', false, true, (s, a) =>  System.Console.WriteLine("Service 2 passed Check3 with: "));
            Register("check4", 'd', true, true, (s, a) => System.Console.WriteLine("Service 2 passed Check4 with: "));
        }
        protected override void Execute(ServiceState state)
        {
            System.Console.WriteLine("Service 2 executed");
        }
    }
    internal class Service3 : Service
    {
        private string content;
        public Service3(string nameSpace, string name) : base(nameSpace, name)
        {
            Register("check1", 'a', true, false, (s, a) => content = a);
        }
        protected override void Execute(ServiceState state)
        {
            if(int.TryParse(content, out int number))
            {
                System.Console.WriteLine("Service 3 executed: " + number);
            }
            else
            {
                List<string> chars = new List<string>();
                System.Console.WriteLine("Service 3 executed: cant parse number");
                state.WriteLine(content);
                chars.Clear();
                for (int i = 0; i < content.Length; i++)
                {
                    if (int.TryParse(content[i].ToString(), out int res))
                        chars.Add(res.ToString());
                    else chars.Add("-");

                }
                state.WriteLine(string.Join("", chars));
            }
        }
    }
    internal class Service4 : Service
    {
        public Service4(string nameSpace, string name) : base(nameSpace, name)
        {
        }
        protected override void Execute(ServiceState state)
        {
            state.Skipp("skipping this service");
        }
    }
    internal class Service5 : Service
    {
        private string content;
        private bool callagain;
        public Service5(string nameSpace, string name) : base(nameSpace, name)
        {
            Register("check1", 'a', true, false, (s, a) => content = a);
            Register("check2", 'b', false, true, (s, a) => callagain = string.IsNullOrWhiteSpace(a));
        }
        protected override void Execute(ServiceState state)
        {
            if (int.TryParse(content, out int servicenumber))
            {
                state.WriteLine("calling service " + content);
                switch (servicenumber)
                {
                    case 1: state.Execute("service1", servicenumber, false); break;
                    case 2: state.Execute($"service2 -a parameter1 -b parameter2 -c -d", servicenumber, false); break;
                    case 3: state.Execute($"service3 -a a8sd7fa8g69ads8f7a0s9d8f7", servicenumber, false); break;
                    case 4:
                    ServiceState second = state.Execute("service4 -a asdf", servicenumber, false);
                    state.WriteLine("executed service 4 from service 5 with success: " +  second.Success.ToString());
                    break;
                    case 5:
                    if (!callagain) state.WriteLine("called this service with dont call again");
                    else state.Execute($"service5 -a {servicenumber} -b", servicenumber, false); break;
                    default: state.WriteLine("no service with number " + servicenumber); break;
                }
            }
            else
            {
                state.WriteLine("cant parse number");
            }
        }
    }
    internal class TestFactory : IFactory<Service>
    {
        public Service Create(Type type)
        {
            try
            {
                return Activator.CreateInstance(type, "", type.Name) as Service;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
    internal class TestCaller : ICaller
    {
        public string Name => "Console";

        public IOutput Output { get; }
        public TestCaller(IOutput output)
        {
            Output = output;
        }
    }
    internal class TestOutput : IErrorLog
    {
        private MessageProvider messages;
        public TestOutput(MessageProvider messages)
        {
            this.messages = messages;
        }
        public void Pass(object sender, string key, Func<string, string> wrapper)
        {
            WriteLine($"{sender}: {wrapper.Invoke(messages.Search(key))}");
        }
        public void Pass(object sender, string key)
        {
            WriteLine($"{sender}: {messages.Search(key)}");
        }
        public void Write(string message)
        {
            System.Console.Write("output: " + message);
        }
        public void WriteLine(string message)
        {
            System.Console.WriteLine("output: " + message);
        }
        public void WriteLine()
        {
            System.Console.WriteLine();
        }
    }
    class Program
    {
        private static ServiceInterpreter interpreter;
        private static MessageProvider messages;
        private static ServiceMapCollection services;
        private static XInitializer initializer;
        private static TestOutput output;
        private static AssemblyHandler handler;
        
        static Program()
        {
            messages = new MessageProvider();
            XInitializer.RegisterDefaultErrors(messages);
            ServiceInterpreter.RegisterDefaultErrors(messages);
            handler = new AssemblyHandler();
            output = new TestOutput(messages);
            services = new ServiceMapCollection();
            ServiceMap map = new ServiceMap("", new TestFactory());
            map.Register(nameof(Service1), typeof(Service1));
            map.Register(nameof(Service2), typeof(Service2));
            map.Register(nameof(Service3), typeof(Service3));
            map.Register(nameof(Service4), typeof(Service4));
            map.Register(nameof(Service5), typeof(Service5));
            services.Register(map);
            interpreter = new ServiceInterpreter(services, output);
            initializer = new XInitializer(handler, output);
            System.Console.BufferHeight = 1000;
        }
        static void Main(string[] args)
        {

            string[] commands = new string[]
            {
                "service1",
                "service1 -a",
                "service1 -a asdfk",
                "service2 -a 1111 -b 2222 -c -d",
                "service2 -a -b 2222 -c -d",
                "service2 -a 1111 -b -c -d",
                "service2 -a 1111 -b 2222 -c 234 -d",
                "service2 -a 1111 -b 2222 -c -d sdf",
                "service2 -b 2222 -c -d",
                "service2 -a 1111 -c -d",
                "service2 -a -b 2222 -d",
                "service2 -a -b 2222 -c",
                "service3 -a 1234",
                "service3 -a asdf",
                "service3 -a as9d80sag70sdfsd",
                "service3 -a   ",
                "service3",
                "service4 -a ",
                "service4 -a -b",
                "service4",
                "service4 -a 234 ",
                "service4 - a 234 -b 132 13",
                "service5 -c 1 ",
                "service5  -a ",
                "service5 -a  ",
                "service5 -a 1",
                "service5 -a 2",
                "service5 -a 3",
                "service5 -a 4",
                "service5 -a 5",
                "service5 -a 1 1 0",
                "service5 - a 10",
                "service5    -a   10",
                "service5 -a 10    ",
                "service5 -a abc    ",
            };

            while(true)
            {
                System.Console.BufferHeight = 1000;
                ConsoleKey key = System.Console.ReadKey().Key;
                if (key == ConsoleKey.D1)
                    CallServiceRoutine(commands);
                else if (key == ConsoleKey.D2)
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Console.testfile.xml"))
                        CallInitializerRoutine(stream);
                }
                else break;
            }

            System.Console.ReadLine();
        }
        private static void CallInitializerRoutine(Stream manieststream)
        {
            IEnumerable< InitializerObject> test = null;
            System.Console.Clear();
            try
            {
                XDocument document = XDocument.Load(manieststream);
                test = initializer.Resolve(document);
            }
            catch(Exception ex)
            {
                System.Console.WriteLine("initializer error");
                System.Console.WriteLine(ex.Message);
            }
        }
        private static void CallServiceRoutine(string[] commands)
        {
            System.Console.Clear();
            TestCaller caller = new TestCaller(output);
            for (int i = 0; i < commands.Length; i++)
            {
                try
                {
                    ServiceOptions options = ServiceOptions.Parse(commands[i], i, true);

                    if (options != null)
                    {
                        ServiceState state = interpreter.Execute(options, caller);
                        if (state.Error)
                        {

                        }
                        else System.Console.WriteLine("succeded");
                    }
                    else System.Console.WriteLine("cant parse command: " + commands[i]);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
                System.Console.WriteLine();
            }
        }
    }
}
