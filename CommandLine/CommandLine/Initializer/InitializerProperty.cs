using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CommandLine.Initializer
{
    public class PropertyBinding
    {
        public bool IsBound { get { return _property != null; } }
        private InitializerObject _source;
        private InitializerProperty _property;
        private readonly string _propertyname;
        public PropertyBinding(InitializerObject source, string propertyname)
        {
            //Type sourcetype = source.GetType();
            if (!source.GetType().IsClass) throw new NotSupportedException();
            if (source is INotifyPropertyChanged)
                ((INotifyPropertyChanged) source).PropertyChanged += HandleChangedEvent;
            _source = source;
            _propertyname = propertyname;
        }
        internal void Attach(InitializerProperty property)
        {
            _property = property;
            object value;
            if (_source.GetValue(_propertyname, out value))
                _property.SetValue(value);
        }
        private void HandleChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            object value;
            if (_source.GetValue(_propertyname, out value))
                _property.SetValue(value);
        }
    }
    public class PropertyChangedCallbackEventArgs
    {
        public object NewValue { get; }
        public object OldValue { get; }
        public PropertyChangedCallbackEventArgs(object newvalue, object oldvalue)
        {
            NewValue = newvalue;
            OldValue = oldvalue;
        }
    }
    public class PropertyMetadata
    {
        public object DefaultValue { get; }
        internal PropertyChangedCallback Callback { get; }
        public PropertyMetadata(object defaultvalue)
        {
            DefaultValue = defaultvalue;
        }
        public PropertyMetadata(object defaultvalue, PropertyChangedCallback callback)
        {
            DefaultValue = defaultvalue;
            Callback = callback;
        }
    }
    public delegate void PropertyChangedCallback(InitializerObject obj, PropertyChangedCallbackEventArgs args);
    public sealed class InitializerProperty
    {
        public string Name { get; }
        public Type Type { get; }
        public PropertyBinding Binding { get { return _binding; } }
        private PropertyMetadata _meta;
        private PropertyBinding _binding;
        private InitializerObject _obj;
        private object _value;
        internal InitializerProperty(string name, Type type, InitializerObject obj, PropertyMetadata meta)
        {
            Name = name;
            Type = type;
            _obj = obj;
            _meta = meta;
            _value = meta.DefaultValue;
        }
        public void Register(PropertyBinding binding)
        {
            _binding = binding;
            _binding.Attach(this);
        }
        internal bool SetValue(object value)
        {
            if (Type.IsClass && value != null && !Type.IsAssignableFrom(value.GetType())) throw new ArgumentException();
            else if (Type.IsValueType && value == null) throw new ArgumentNullException();
            else if (!Equals(_value, value))
            {
                _meta.Callback?.Invoke(_obj, new PropertyChangedCallbackEventArgs(value, _value));
                _value = value;
                return true;
            }
            return false;
        }
        public object GetValue()
        {
            return _value;
        }
    }
}
