using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
namespace CommandLine.Initializer
{
    public class AssemblyHandler
    {
        public event EventHandler<AssemblyLoadEventArgs> Loaded;
        private Dictionary<string, Assembly> _assemblies;
        private static readonly Dictionary<Assembly, XDocument> _documents = new Dictionary<Assembly, XDocument>();
        public AssemblyHandler()
        {
            _assemblies = new Dictionary<string, Assembly>();
        }
        public Type GetType(string assemblyName, string fullnamespace, string typeName)
        {
            Assembly assembly = Load(assemblyName);
            return assembly.GetType($"{fullnamespace}.{typeName}");
        }
        public T Create<T>(string assemblyname, string fullNamespace, string typeName, params object[] args) where T : class
        {
            Assembly assembly = Load(assemblyname);
            Type type = assembly.GetType($"{fullNamespace}.{typeName}", true, true);
            T instance = Activator.CreateInstance(type, args) as T;
            return instance;
        }
        public Assembly Load(string name)
        {
            name = name.ToLower();
            if (_assemblies.ContainsKey(name)) return _assemblies[name];
            Assembly assembly;
            if(!_assemblies.TryGetValue(name, out assembly))
            {
                assembly = Assembly.Load(name);
                _assemblies.Add(assembly.GetName().Name.ToLower(), assembly);
                Loaded?.Invoke(this, new AssemblyLoadEventArgs(assembly));
            }
            return assembly;
        }
        public bool Add(Assembly assembly)
        {
            string name = assembly.GetName().Name;
            bool result = !_assemblies.ContainsKey(name);
            if (result)
            {
                _assemblies.Add(name, assembly);
                Loaded?.Invoke(this, new AssemblyLoadEventArgs(assembly));
            }
            return result;
        }
        public IEnumerable<Assembly> GetAsseblies()
        {
            return _assemblies.Values;
        }
        public static XDocument GetDocument(Assembly assembly)
        {
            lock(_documents)
            {
                if (_documents.ContainsKey(assembly)) return _documents[assembly];
                string descriptionpath = assembly.Location;
                descriptionpath = descriptionpath.Remove(descriptionpath.Length - 3, 3) + "xml";
                XDocument document = null;
                if (File.Exists(descriptionpath)) document = XDocument.Load(descriptionpath);
                if (document != null) _documents.Add(assembly, document);
                return document;
            }
        }
    }
}
