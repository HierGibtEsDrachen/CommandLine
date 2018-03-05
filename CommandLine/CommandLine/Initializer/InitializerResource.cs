using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
namespace CommandLine.Initializer
{
    [ContentProperty(nameof(Content), "add")]
    public class InitializerResource : InitializerObject
    {
        public IReadOnlyDictionary<string, object> Dictionary { get { return Content; } }
        internal ResourceContent Content { get { return (ResourceContent) _content.GetValue(); } }
        private readonly InitializerProperty _content;
        public InitializerResource()
        {
            _content = new InitializerProperty(nameof(Content), typeof(ResourceContent), this, new PropertyMetadata(new ResourceContent()));
        }
        public bool TryFindResource(string key, out object resource)
        {
            return Content.TryGetValue(key, out resource);
        }
        public object this[string key]
        {
            get { return ((ResourceContent)_content.GetValue())[key]; }
        }
    }
    internal class ResourceContent : InitializerObject, IReadOnlyDictionary<string, object>
    {
        public IEnumerable<string> Keys { get { return _resource.Keys; } }
        public IEnumerable<object> Values { get { return _resource.Values; } }
        private Dictionary<string, object> _resource;
        private readonly InitializerMethod _addmethod;
        public ResourceContent()
        {
            _resource = new Dictionary<string, object>();
            _addmethod = Register("add", null, new MethodMetadata(Add, typeof(string), typeof(object)));
        }
        private object Add(object[] args)
        {
            if (args.Length != 2) throw new IndexOutOfRangeException();
            else if (args[0] is string key) _resource.Add(key, args[1]);
            else throw new ArgumentException();
            return null;
        }
        public object this[string key] { get { return _resource[key]; } }
        public int Count { get { return _resource.Count; } }
        public bool ContainsKey(string key)
        {
            return _resource.ContainsKey(key);
        }
        public bool TryGetValue(string key, out object value)
        {
            return _resource.TryGetValue(key, out value);
        }
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _resource.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _resource.GetEnumerator();
        }
    }
}
