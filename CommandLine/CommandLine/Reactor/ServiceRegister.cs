using System.Collections;
using System.Collections.Generic;
namespace CommandLine.Reactor
{
    public class ServiceRegister : IEnumerable<Service>
    {
        public string Name { get { return _register.Name; } }
        public IEnumerable<string> Keys { get { return _keys; } }
        private List<string> _keys;
        private Register<Service> _register;
        public ServiceRegister(string name)
        {
            _keys = new List<string>();
            _register = new Register<Service>(name);
        }
        public Service Find(string name)
        {
            return _register.Find(name);
        }
        public bool Contains(string name)
        {
            return _register.Contains(name);
        }
        public bool Contains(Service service)
        {
            return _register.Contains(service);
        }
        public void Accept(Service service)
        {
            _register.Add(service);
            _keys.Add(service.Name);
        }
        public IEnumerator<Service> GetEnumerator()
        {
            return _register.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _register.GetEnumerator();
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
