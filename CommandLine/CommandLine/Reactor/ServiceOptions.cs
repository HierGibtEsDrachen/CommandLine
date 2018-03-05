using Extensions;
using System;
using System.Collections.Generic;
namespace CommandLine.Reactor
{
    public class ServiceOptions
    {
        #region Parsing
        private const char _space = ' ';
        private const char _mark = '-';
        private const char _equal = '=';
        private const char _splitter = ':';
        public static ServiceOptions Parse(string input, int id, bool fullinformation)
        {
            StringParser parser = new StringParser(input);
            parser.SkipIf(_space);
            string fullname = parser.ReadUntil(_space, _mark).ToLower();
            string[] splittedName = fullname.Split(StringSplitOptions.RemoveEmptyEntries, _splitter);
            ServiceOptions options;
            if (splittedName.Length > 2)
                return null; // throw new NotSupportedException($"{nameof(Parse)} - Das Format wird nicht unterstützt.");
            else if (splittedName.Length > 1) options = new ServiceOptions(input, splittedName[0], splittedName[1], id, fullinformation);
            else if (splittedName.Length > 0) options = new ServiceOptions(input, string.Empty, splittedName[0], id, fullinformation);
            else return null; //throw new NotImplementedException($"{nameof(Parse)} - kein Name.");
            while (!parser.End)
            {
                parser.SkipUntil(_mark);
                if (parser.Peek(1) == _mark)
                {
                    string argname = parser.SkipIf(_mark).SkipIf(_space).ReadUntil(_equal, _mark).ToLower();
                    string argparam = string.Empty;
                    if (parser.Peek() == _equal)
                    {
                        argparam = parser.Skip(_equal, 1).ReadUntilNext(_mark).ToLower();
                        if (parser.Peek() != _space) argparam += parser.ReadUntil(_mark).ToLower();
                    }
                    options.AddArgs(argname, argparam, true);
                }
                else
                {
                    bool isschortcut = true;
                    string argname = parser.SkipIf(_mark).SkipIf(_space).ReadUntilNext(_space, _mark).ToLower();
                    string argparam = parser.SkipIf(_space).ReadUntilNext(_mark).ToLower();
                    if (parser.Peek() != _space) argparam += parser.ReadUntil(_mark);
                    if (argname.Length == 0)
                    {
                        StringParser parser2 = new StringParser(argparam);
                        argname = parser2.SkipIf(_space).ReadUntilAndSkip(_space);
                        argparam = parser2.ReadToEnd();
                    }
                    else if (argname.Contains(_equal.ToString()))
                    {
                        StringParser parser2 = new StringParser(argname);
                        argname = parser2.SkipIf(_space).ReadUntilAndSkip(_equal);
                        argparam = parser2.ReadToEnd() + argparam;
                        isschortcut = false;
                    }
                    options.AddArgs(argname, argparam, !isschortcut);
                }
            }
            return options;
        }
        #endregion
        public string Command { get; }
        public string Fullname { get; }
        public string Namespace { get; }
        public string Name { get; }
        public bool HasArgs { get { return _args.Count > 0; } }
        public int ID { get; }
        public bool FullInformation { get; }
        public IEnumerable<ArgumentInfo> Args { get { return _args; } }
        List<ArgumentInfo> _args;
        public ServiceOptions(string command, string nameSpace, string name, int id, bool fullinformation, IEnumerable<ArgumentInfo> args) :
            this(command, nameSpace, name, id, fullinformation)
        {
            if (args != null) _args.AddRange(args);
        }
        public ServiceOptions(string command, string nameSpace, string name, int servicecallerid, bool fullinformation)
        {
            Command = command;
            Name = name;
            Namespace = nameSpace;
            ID = servicecallerid;
            FullInformation = fullinformation;
            if (nameSpace.NullOrEmpty()) Fullname = name;
            else Fullname = $"{Namespace}:{Name}";
            _args = new List<ArgumentInfo>();
        }
        internal void AddArgs(ArgumentInfo arg)
        {
            _args.Add(arg);
        }
        internal void AddArgs(string name, string param, bool isfullname)
        {
            _args.Add(new ArgumentInfo(name, param, isfullname));
        }
        public override string ToString()
        {
            if (Namespace.NotNullOrEmpty()) return $"{Namespace}:{Name}";
            else return Name;
        }
    }
}
