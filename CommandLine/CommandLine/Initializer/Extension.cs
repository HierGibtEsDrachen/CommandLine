using System;
namespace CommandLine.Initializer
{
    public delegate bool SourceHandler(string key, out ISource source);
    public delegate (NamespaceDeclaration declaration, string value) NamespaceHandler(string assemblykey);
    public delegate bool ExtensionResolver(NestedArgument argument, Type targettype, out object obj);
    public abstract class Extension
    {
        public string Name { get; }
        public Extension(string name) { Name = name; }
        public abstract object Resolve(NestedArgument argument, SourceHandler source,
            NamespaceHandler scope, ExtensionResolver resolver, Type targettype);
    }
}
