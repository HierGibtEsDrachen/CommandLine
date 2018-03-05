using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CommandLine.Initializer
{
    public class InitializerSource : InitializerObject
    {
        public string Name { get; }
        public InitializerSource(string name) { Name = name; }
        public object GetElement(string assembly, string @namespace, string key)
        {
            throw new NotImplementedException();
        }
        public T GetElement<T>(string assembly, string @namespace, string key)
        {
            throw new NotImplementedException();
        }
        public object GetElement(NamespaceDeclaration declaration, string key)
        {
            throw new NotImplementedException();
        }
        public object GetElement<T>(NamespaceDeclaration declaration, string key)
        {
            throw new NotImplementedException();
        }
    }
}
