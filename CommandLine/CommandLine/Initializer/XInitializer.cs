using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Collections;
using CommandLine.Reactor;

namespace CommandLine.Initializer
{
    public class XInitializer
    {
        public static void RegisterDefaultErrors(MessageProvider provider)
        {
            if (provider != null)
            {
                provider.Register("initializer/create/failed", "cant create instance: {0} from declaration: {1}, exception: {2}");
                provider.Register("initializer/instance/type", "instance: {0} is not of type: {1}");
                provider.Register("initializer/instance/noproperty", "property {0} not found on {1} type={2}.");
                provider.Register("initializer/resource/notfound", "no resource with key: {0}");
                provider.Register("initializer/arg/nullorempty", "the given value is null or empty: {0}");
                provider.Register("initializer/format/chained", "chained expressions not supported: {1}");
                provider.Register("initializer/contentmethod/notfound", "no add method on: {0} on object: {1}");
                provider.Register("initializer/contentproperty/unexpected", "unexpected error instance: {0} no contentsource.");
                provider.Register("initializer/extension/notfound", "no extension with name: {0}");
                provider.Register("initializer/extension/failed", "extension: {0} failed.");
                provider.Register("initializer/element/nokeyattribute", "no key attribute on: {0}");
                provider.Register("initializer/element/nocontent", "{0} doesnt supoort content");
                provider.Register("initializer/declaration/nonamespace", "no namespace on: {0}");
                provider.Register("initializer/declaration/novalue", "no value on: {0}");
                provider.Register("initializer/declaration/notfound", "no namespace declaration found with prefix: {0}");
                provider.Register("initializer/savechangetype/failed", "cant change: {0} to type: {1}.{2}");
            }
        }
        public bool IsClear { get { return _currentelement.Count == 0; } }
        private AssemblyHandler _assemblies;
        private Stack<StackElement> _currentelement;
        private Dictionary<string, NamespaceDeclaration> _assemblyKeys;
        private Dictionary<string, Extension> _extensions;
        private Dictionary<string, ISource> _sources;
        private IErrorLog _debug;
        public XInitializer(AssemblyHandler assemblies, IErrorLog debug)
        {
            _debug = debug;
            _assemblies = assemblies;
            _currentelement = new Stack<StackElement>();
            _assemblyKeys = new Dictionary<string, NamespaceDeclaration>();
            _sources = new Dictionary<string, ISource>();
            _extensions = new Dictionary<string, Extension>();
        }
        public void Reset()
        {
            _currentelement.Clear();
        }
        public bool Register(Extension extension, bool replace = false)
        {
            bool result;
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            string key = extension.Name.ToLower();
            if (result = !_extensions.ContainsKey(key)) _extensions.Add(key, extension);
            else if (replace) _extensions[key] = extension;
            return result || replace;
        }
        public bool Register(ISource source, bool replace = false)
        {
            bool result;
            if (source == null) throw new ArgumentNullException(nameof(source));
            string key = source.Name.ToLower();
            if (result = !_sources.ContainsKey(key)) _sources.Add(key, source);
            else if (replace) _sources[key] = source;
            return result || replace;
        }
        public IEnumerable<InitializerObject> Resolve(XDocument document)
        {
            foreach(XAttribute attribute in document.Root.Attributes())
                if(attribute.IsNamespaceDeclaration && 
                    NamespaceDeclaration.Resolve(attribute.Name.NamespaceName, attribute.Value, out NamespaceDeclaration declaration))
                    if (!_assemblyKeys.ContainsKey(declaration.Token)) _assemblyKeys.Add(declaration.Token, declaration);
            List<InitializerObject> objects = new List<InitializerObject>();
            foreach(XElement element in document.Root.Elements())
            {
                try
                {
                    InitializerObject instance = Resolve(element);
                    if (instance != null) objects.Add(instance);
                    //TODO: error log
                }
                catch (Exception)
                {
                    //TODO: error log
                }
            }
            return objects;
        }
        public InitializerObject Resolve(XElement element)
        {
            StackElement stackelement = new StackElement
            {
                Element = element ?? throw new ArgumentNullException(nameof(element))
            };
            if (NamespaceDeclaration.Resolve(element.GetPrefixOfNamespace(element.Name.Namespace),
                element.Name.NamespaceName, out NamespaceDeclaration declaration))
            {
                if (!_assemblyKeys.ContainsKey(declaration.Token)) _assemblyKeys.Add(declaration.Token, declaration);
                stackelement.Token = declaration.Token;
            }
            object inst = null;
            try
            {
                inst = _assemblies.Create<object>(declaration.Assembly, declaration.Namespace, element.Name.LocalName);
            }
            catch(Exception exception)
            {
                _debug?.Pass(this, "instance/create/failed", (s) => string.Format(s, element.Name.LocalName, declaration, exception));
            }
            if (inst is InitializerObject instance)
            {
                stackelement.Instance = instance;
                _currentelement.Push(stackelement);
                if (element.HasAttributes)
                {
                    foreach (XAttribute attri in element.Attributes())
                        ResolveAttributes(instance, attri);
                }
                if (element.HasElements)
                {
                    foreach(XElement child in element.Elements())
                        ResolveElements(instance, element, child, instance.GetType().
                            GetCustomAttribute(typeof(ContentPropertyAttribute)) as ContentPropertyAttribute);
                }
                    
                StackElement state = _currentelement.Pop();
                return instance;
            }   
            else _debug?.Pass(this, "initializer/instance/type", (s) => string.Format(s, element.Name.LocalName, declaration));

            return null;
        }
        private bool ResolveExtension(NestedArgument argument, Type targettype, out object obj)
        {
            if (GetExtension(argument.Scope.ToLower(), out Extension extension))
                obj = extension.Resolve(argument, GetSource, Parse, ResolveExtension, targettype);
            else obj = null;
            return obj != null;
        }
        private bool GetKey(XElement node, out string key)
        {
            if (node.Attribute("Key") != null)
            {
                key = node.Attribute("Key").Value;
            }
            else if (node.Attribute("key") != null)
            {
                key = node.Attribute("key").Value;
            }
            else
            {
                _debug?.Pass(this, "initializer/element/nokeyattribute", (s) => string.Format(s, node.Name));
                key = null;
                return false;
            }
            return !string.IsNullOrWhiteSpace(key);
        }
        private object GenericResolve(XElement element)
        {
            string value = element.Value.TrimStart().TrimEnd();
            switch (element.Name.LocalName.ToLower())
            {
                case "string": return value;
                case "boolean":
                case "bool": return Convert.ChangeType(value, typeof(bool));
                case "byte": return Convert.ChangeType(value, typeof(byte));
                case "sbyte": return Convert.ChangeType(value, typeof(sbyte));
                case "short":                           
                case "int16": return Convert.ChangeType(value, typeof(short));
                case "int":                             
                case "int32": return Convert.ChangeType(value, typeof(int));
                case "long":                            
                case "int64": return Convert.ChangeType(value, typeof(long));
                case "float":
                case "single": return Convert.ChangeType(value, typeof(float));
                case "double": return Convert.ChangeType(value, typeof(double));
                default: return Resolve(element);
            }
        }
        private NamespaceDeclaration GetDeclaration(string assemblykey)
        {
            if(!string.IsNullOrWhiteSpace(assemblykey))
            {
                _debug?.Pass(this, "initializer/arg/nullorempty", (s) => string.Format(s, nameof(assemblykey)));
                return null;
            }
            if (!_assemblyKeys.TryGetValue(assemblykey, out NamespaceDeclaration assembly))
            {
                foreach(XElement ele in _currentelement.Peek().Element.AncestorsAndSelf())
                {
                    foreach(XAttribute attribute in ele.Attributes())
                    {

                        if (attribute.IsNamespaceDeclaration && !_assemblyKeys.ContainsKey(attribute.Name.LocalName) &&
                            NamespaceDeclaration.Resolve(attribute.Name.LocalName, attribute.Value, out NamespaceDeclaration declaration))
                        {
                            if (declaration.Assembly == assemblykey) assembly = declaration;
                            _assemblyKeys.Add(attribute.Name.LocalName, declaration);
                        }
                    }
                }
            }            
            if(assembly == null) _debug?.Pass(this, "initializer/declaration/notfound", (s) => string.Format(s, assemblykey));
            return assembly;
        }
        private (NamespaceDeclaration declartation, string value) Parse(string value)
        {
            NamespaceDeclaration declaration = null;
            if (value.Contains(':'))
            {
                StringParser parser = new StringParser(value);
                string namespaceKey = parser.ReadUntilAndSkip(':');
                if (string.IsNullOrWhiteSpace(namespaceKey))
                {
                    _debug?.Pass(this, "initializer/declaration/nonamespace", (s) => string.Format(s, value));
                }
                else declaration = GetDeclaration(namespaceKey);
                value = parser.ReadToEnd();
                if (string.IsNullOrWhiteSpace(value))
                {
                    _debug?.Pass(this, "initializer/declaration/novalue", (s) => string.Format(s, value));
                }
            }
            return (declaration, value);
        }
        private bool GetSource(string key, out ISource source)
        {
            bool resutl = _sources.TryGetValue(key, out source);
            if (!resutl) _debug?.Pass(this, "initializer/resource/notfound", (s) => string.Format(s, key));
            return resutl;
        }
        private bool GetExtension(string key, out Extension extension)
        {
            bool resutl = _extensions.TryGetValue(key, out extension);
            if(!resutl) _debug?.Pass(this, "initializer/extension/notfound", (s) => string.Format(s, key));
            return resutl;
        }
        private void ResolveAttributes(InitializerObject instance, XAttribute attribute)
        {
            string propertyname = attribute.Name.LocalName;
            if (attribute.IsNamespaceDeclaration && !_assemblyKeys.ContainsKey(propertyname))
            {
                if (NamespaceDeclaration.Resolve(propertyname, attribute.Value, out NamespaceDeclaration declaration))
                    _assemblyKeys.Add(propertyname, declaration);
            }
            else
            {
                int count = NestedArgument.ResolveNestedParenthesses(attribute.Value, '{', '}', out IEnumerable<NestedArgument> nestedargument);
                if (!instance.GetProperty(propertyname, out InitializerProperty property))
                    _debug?.Pass(this, "initializer/instance/noproperty", (s) => string.Format(s, propertyname, instance, instance.GetType()));
                else if (count > 1) _debug?.Pass(this, "initializer/format/chained", (s) => string.Format(s, attribute.Value));
                else if (count == 1)
                {
                    NestedArgument argument = nestedargument.First();
                    if (ResolveExtension(argument, property.Type, out object obj))
                    {
                        if (obj is PropertyBinding binding) property.Register(binding);
                        else if (obj != null) property.SetValue(obj);
                    }
                    else _debug?.Pass(this, "initializer/extension/failed", (s) => string.Format(argument.Scope));
                }
                else if (count == 0 && SaveChangeType(property.Type, attribute.Value, out object obj)) property.SetValue(obj);
            }
        }

        private bool SaveChangeType(Type type, string value, out object obj)
        {
            try
            {
                obj = Convert.ChangeType(value, type);
                return true;
            }
            catch(Exception)
            {
                obj = null;
                _debug?.Pass(this, "initializer/savechangetype/failed", s => string.Format(s, value, type.Namespace, type.Name));
                return false;
            }
        }

        private void ResolveElements(InitializerObject instance, XElement parent, XElement node, ContentPropertyAttribute contentproperty)
        {
            string propertysource;
            string[] nodename = node.Name.LocalName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (nodename.Length > 2)
            {
                //TODO: loggen
                return;
            }
            else if (nodename.Length == 2 && string.Equals(nodename[0], parent.Name.LocalName)) propertysource = nodename[1];
            else if (contentproperty == null)
            {
                _debug?.Pass(this, "initializer/element/nocontent", (s) => string.Format(s, parent.Name));
                propertysource = null;
            }
            else propertysource = contentproperty.Name;
            if(!string.IsNullOrWhiteSpace(propertysource))
            {
                if (instance.GetProperty(propertysource, out InitializerProperty property))
                {
                    object propertyvalue = property.GetValue();
                    if (propertyvalue is IAddContent propertyinterface)
                    {
                        if (nodename.Length == 2)
                        {
                            foreach (XElement ele in node.Elements())
                                propertyinterface.Add(GetObject(ele, propertyinterface));
                        }
                        else propertyinterface.Add(GetObject(node, instance));
                    }
                    else if (propertyvalue is ResourceContent)
                    {
                        InitializerObject proptertyobject = propertyvalue as InitializerObject;
                        if (contentproperty != null && propertysource == contentproperty.Name) CallDictionaryAdd(node, instance, null, (n, v) => proptertyobject.Invoke(contentproperty.MethodName, out object res, v));
                        else
                        {
                            foreach (XElement ele in node.Elements())
                            {
                                CallDictionaryAdd(ele, instance, null, (n, v) => 
                                proptertyobject.Invoke(contentproperty.MethodName, out object res, v));
                            }
                        }
                    }
                    else if (propertyvalue is InitializerObject propertyobject)
                    {
                        if (nodename.Length == 2)
                        {
                            if (propertyobject.HasMethod(contentproperty.MethodName))
                            {
                                if (contentproperty != null && propertysource == contentproperty.Name) propertyobject.Invoke(contentproperty.MethodName, out object res, GetObject(node, instance));
                                else
                                {
                                    foreach(XElement ele in node.Elements())
                                    {
                                        propertyobject.Invoke(contentproperty.MethodName, out object res, GetObject(ele, instance));
                                    }
                                }
                            }
                            else _debug?.Pass(this, "initializer/contentmethod/notfound", (s) => string.Format(s, propertyvalue, instance));
                        }
                        else if(!propertyobject.Invoke("Add", out object res, GetObject(node, instance)))
                            _debug?.Pass(this, "initializer/contentmethod/notfound", (s) => string.Format(s, propertyvalue, instance));
                    }
                    else if (property.Type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>) || i == typeof(IDictionary)))
                    {
                        MethodInfo method = property.Type.GetMethods().FirstOrDefault(m => m.Name.ToLower() == "add" && m.GetParameters().Count() == 2);
                        if (method == null) _debug?.Pass(this, "initializer/contentmethod/notfound", (s) => string.Format(s, propertyvalue, instance));
                        else if (contentproperty != null && propertysource == contentproperty.Name) CallDictionaryAdd(node, instance, propertyvalue, method.Invoke);
                        else
                        {
                            foreach(XElement ele in node.Elements())
                            {
                                CallDictionaryAdd(ele, instance, propertyvalue, method.Invoke);
                            }
                        }
                    }
                    else if (property.Type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>) || i == typeof(ICollection)))
                    {
                        MethodInfo method = property.Type.GetMethods().FirstOrDefault(m => m.Name.ToLower() == "add" && m.GetParameters().Count() == 1);
                        if (method == null) _debug?.Pass(this, "initializer/contentmethod/notfound", (s) => string.Format(s, propertyvalue, instance));
                        else if (contentproperty != null && propertysource == contentproperty.Name) method.Invoke(propertyvalue, new[] { GetObject(node, instance) });
                        else
                        {
                            foreach(XElement ele in node.Elements())
                            {
                                method.Invoke(propertyvalue, new[] { GetObject(ele, instance) });
                            }
                        }
                    }
                    else
                    {
                        //InitializerProperty initializerproperty;
                        if (node.HasElements) instance.SetValue(propertysource, GetObject(node.Elements().FirstOrDefault(), instance));
                        else if (instance.GetProperty(nodename[1], out InitializerProperty initializerproperty))
                            if(SaveChangeType(initializerproperty.Type, (node.FirstNode as XText).Value.TrimStart('\n', '\r', ' ').TrimEnd('\n', '\r', ' '), out object value))
                                initializerproperty.SetValue(value);
                    }
                }
                else _debug?.Pass(this, "initializer/contentproperty/unexpected", s => string.Format(s, instance));
            }
        }
        private void CallDictionaryAdd(XElement parent, InitializerObject instance, object reflobj, Func<object, object[], object> method)
        {
            if (GetKey(parent, out string key))
                method.Invoke(reflobj, new[] { key, GetObject(parent, instance) });
        }
        private object GetObject(XElement node, object parent)
        {
            object obj = GenericResolve(node);
            if (obj is InitializerObject) ((InitializerObject) obj).Owner = parent;
            return obj;
        }
        private class StackElement
        {
            public string Token { get; set; }
            public XElement Element { get; set; }
            public InitializerObject Instance { get; set; }
        }
    }
}
