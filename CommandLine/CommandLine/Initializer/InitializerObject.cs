using System;
using System.Collections.Generic;
namespace CommandLine.Initializer
{
    public abstract class InitializerObject
    {
        public object Owner { get; internal set; }
        private Dictionary<string, InitializerProperty> _properties;
        private Dictionary<string, InitializerMethod> _methods;
        public InitializerObject()
        {
            _properties = new Dictionary<string, InitializerProperty>();
            _methods = new Dictionary<string, InitializerMethod>();
        }
        internal bool SetValue(string name, object value)
        {
            bool result = false;
            InitializerProperty property;
            if (result = _properties.TryGetValue(name, out property))
                property.SetValue(value);
            return result;
        }
        internal bool GetValue(string name, out object value)
        {
            bool result;
            if (result = _properties.TryGetValue(name, out InitializerProperty property))
                value = property.GetValue();
            else value = null;
            return result;
        }
        internal bool GetProperty(string name, out InitializerProperty property)
        {
            return _properties.TryGetValue(name, out property);
        }
        internal bool HasMethod(string methodName)
        {
            return _methods.ContainsKey(methodName);
        }
        internal bool Invoke(string name, out object result, params object[] args)
        {
            if (_methods.TryGetValue(name, out InitializerMethod method))
            {
                result = method.Invoke(args);
                return true;
            }
            result = null;
            return false;
        }
        protected InitializerMethod Register(string methodname, Type returntype, MethodMetadata meta)
        {
            if (methodname == null) throw new ArgumentNullException(nameof(methodname));
            if (meta == null) throw new ArgumentNullException(nameof(meta));
            if (_properties.ContainsKey(methodname)) throw new ArgumentException();
            InitializerMethod method = new InitializerMethod(methodname, returntype, this, meta);
            _methods.Add(methodname, method);
            return method;
        }
        protected InitializerProperty Register(string propertyname, Type propertytype, PropertyMetadata meta)
        {
            if (propertyname == null) throw new ArgumentNullException(nameof(propertyname));
            if (propertytype == null) throw new ArgumentNullException(nameof(propertytype));
            if (meta == null) throw new ArgumentNullException(nameof(meta));
            if (_properties.ContainsKey(propertyname)) throw new ArgumentException();            
            InitializerProperty property = new InitializerProperty(propertyname, propertytype, this, meta);
            _properties.Add(propertyname, property);
            return property;
        }
    }
}