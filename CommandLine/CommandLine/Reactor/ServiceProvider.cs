using System;
using System.Collections.Generic;
namespace CommandLine.Reactor
{
    public class ServiceMap : IServiceProvider
    {
        public string Namespace { get; }
        public IEnumerable<string> Keys { get { return _keyedtypes.Keys; } }
        public IEnumerable<string> Names { get { return _namedtypes.Keys; } }
        public IEnumerable<Type> Types { get { return _keyedtypes.Values; } }
        public bool IsSealed
        {
            get { return _sealed; }
            set { if (value) _sealed = value; }
        }
        private Dictionary<string, Type> _keyedtypes;
        private Dictionary<string, Type> _namedtypes;
        private bool _sealed;
        private IFactory<Service> _provider;
        public ServiceMap(string nameSpace, IFactory<Service> provider)
        {
            Namespace = nameSpace;
            _provider = provider;
            _keyedtypes = new Dictionary<string, Type>();
            _namedtypes = new Dictionary<string, Type>();
        }
        public bool ContainsKey(string key)
        {
            return _keyedtypes.ContainsKey(key.ToLower());
        }
        public bool ContainsName(string name)
        {
            return _namedtypes.ContainsKey(name.ToLower());
        }
        public bool Register(string key, Type type)
        {
            if (IsSealed) return false;
            bool result = false;
            key = key.ToLower();
            if (!_keyedtypes.ContainsKey(key) && !_namedtypes.ContainsKey(type.Name.ToLower()))
            {
                _keyedtypes.Add(key, type);
                _namedtypes.Add(type.Name.ToLower(), type);
                result = true;
            }
            return result;
        }
        public Type FindByKey(string key)
        {
            if (_keyedtypes.TryGetValue(key.ToLower(), out Type type)) return type;
            else return null;
        }
        public Type FindByName(string name)
        {
            if (_namedtypes.TryGetValue(name.ToLower(), out Type type)) return type;
            else return null;
        }
        public T FindCreateByKey<T>(string key) where T : class
        {
            return _provider.Create(FindByKey(key)) as T;
        }
        public T FindCreateByName<T>(string name) where T : class
        {
            return _provider.Create(FindByName(name)) as T;
        }
        public object GetService(Type serviceType)
        {
            return FindByName(serviceType.Name);
        }
    }
    public class ServiceMapCollection
    {
        public IEnumerable<string> Namespaces { get { return _provider.Keys; } }
        public IEnumerable<ServiceMap> Maps { get { return _provider.Values; } }
        public bool IsSealed
        {
            get { return _sealed; }
            set { if (value) _sealed = value; }
        }
        private Dictionary<string, ServiceMap> _provider;
        private bool _sealed;
        public ServiceMapCollection()
        {
            _provider = new Dictionary<string, ServiceMap>();
        }
        public void Register(string @namespace, string servicekey, Type type)
        {
            if (_provider.TryGetValue(@namespace.ToLower(), out ServiceMap map))
            {
                map.Register(servicekey, type);
            }
        }
        public bool Register(ServiceMap map)
        {
            if (IsSealed) return false;
            string key = map.Namespace.ToLower();
            if (!_provider.ContainsKey(key))
            {
                _provider.Add(key, map);
                return true;
            }
            else return false;
        }
        public bool Contains(string nameSpace)
        {
            return _provider.ContainsKey(nameSpace.ToLower());
        }
        public ServiceMap Find(string nameSpace)
        {
            if (_provider.TryGetValue(nameSpace.ToLower(), out ServiceMap map))
                return map;
            else return null;
        }
        public Type Find(string nameSpace, string key)
        {
            ServiceMap map = Find(nameSpace);
            if (map != null) return map.FindByKey(key);
            return null;
        }
        public T FindCreateByKey<T>(string nameSpace, string key) where T : class
        {
            ServiceMap map = Find(nameSpace);
            return map?.FindCreateByKey<T>(key);
        }
        public T FindCreateByName<T>(string nameSpace, string key) where T : class
        {
            ServiceMap map = Find(nameSpace);
            return map?.FindCreateByName<T>(key);
        }
    }
}
