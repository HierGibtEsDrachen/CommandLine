using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine.Initializer
{
    public class NamespaceDeclaration
    {
        public string Assembly { get; private set; }
        public string Namespace { get; private set; }
        public string Token { get; private set; }
        private NamespaceDeclaration(string token, string assembly, string nameSpace)
        {
            Token = token;
            Assembly = assembly;
            Namespace = nameSpace;
        }
        public static bool Resolve(string token, string value, out NamespaceDeclaration declaration)
        {
            declaration = null;
            string assembly = string.Empty;
            string space = string.Empty;
            string[] splittedvalue = value.Split(StringSplitOptions.RemoveEmptyEntries, ';', ':');

            if (splittedvalue.Length == 0)
                return false;
            int index = Array.IndexOf(splittedvalue, "assembly");
            if (splittedvalue.Length >= index + 1)
                assembly = splittedvalue[index + 1];

            index = Array.IndexOf(splittedvalue, "namespace");
            if (splittedvalue.Length >= index + 1)
                space = splittedvalue[index + 1];

            if (space.NullOrEmpty())
                return false;

            declaration = new NamespaceDeclaration(token, assembly, space);
            return true;
        }
        public override string ToString()
        {
            return $"{Token}=\"assembly:{Assembly};namespace:{Namespace}\"";
        }
    }
}
