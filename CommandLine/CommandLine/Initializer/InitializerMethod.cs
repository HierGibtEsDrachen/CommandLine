using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CommandLine.Initializer
{
    public delegate object MethodDelegate(params object[] args);
    public class MethodInvokeCallbackEventArgs
    {
        public object[] Arguments { get; }
        public MethodInvokeCallbackEventArgs(object[] args)
        {
            Arguments = args;
        }
    }
    public delegate void MethodCallback(InitializerObject parent, MethodInvokeCallbackEventArgs args);
    public sealed class MethodMetadata
    {
        internal MethodDelegate Delegate { get; }
        internal MethodCallback Callback { get; }
        public Type[] InputTypes { get; }
        public bool Validate { get; }
        public MethodMetadata(MethodDelegate methoddelegate, params Type[] inputtypes) : this(methoddelegate, null, false, inputtypes)
        {
        }
        public MethodMetadata(MethodDelegate methoddelegate, MethodCallback callback, params Type[] inputtypes)
            : this(methoddelegate, callback, false, inputtypes)
        {
        }
        public MethodMetadata(MethodDelegate methoddelegate, MethodCallback callback, bool validate, params Type[] inputtypes)
        {
            Delegate = methoddelegate;
            InputTypes = inputtypes;
            Callback = callback;
            Validate = validate;
        }
    }
    public sealed class InitializerMethod
    {
        public string Name { get; }
        public Type ReturnType { get; }
        private MethodMetadata _meta;
        private InitializerObject _parent;
        internal InitializerMethod(string name, Type returntype, InitializerObject parent, MethodMetadata meta)
        {
            Name = name;
            ReturnType = returntype;
            _meta = meta;
            _parent = parent;
        }
        public object Invoke(params object[] args)
        {
            if (args.Length != _meta.InputTypes.Length) throw new ArgumentOutOfRangeException();
            if(_meta.Validate)
            {
                for (int i = 0; i < _meta.InputTypes.Length; i++)
                {
                    Type type = args[i].GetType();
                    if (!_meta.InputTypes[i].Equals(type) && !_meta.InputTypes[i].IsAssignableFrom(type))
                        throw new ArgumentException();
                }
            }
            _meta.Callback?.Invoke(_parent, new MethodInvokeCallbackEventArgs(args));
            return _meta.Delegate.Invoke(args);
        }
    }
}
