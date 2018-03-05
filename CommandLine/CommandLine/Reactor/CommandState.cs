using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine.Reactor
{

    public class CommandState : IOutput
    {
        public bool HasResult { get { return Result != null; } }
        public Type ResultType { get { return Result?.GetType(); } }
        public object Result { get; set; }
        public bool CanWrite { get { return _output != null; } }
        public bool RuntimeError
        {
            get { return _error; }
            set
            {
                _error |= value;
            }
        }
        public bool ParseError
        {
            get { return _parseError; }
            set
            {
                _parseError |= value;
            }
        }
        public bool Error { get { return _error || _parseError; } }
        public bool Success { get { return !Error; } }
        private bool _error;
        private bool _parseError;
        private IOutput _output;
        public virtual void WriteLine()
        {
            _output?.WriteLine("");
        }
        public virtual void WriteLine(string message)
        {
            _output?.WriteLine(message);
        }
        public virtual void Write(string message)
        {
            _output?.Write(message);
        }
        public void Skipp(string message)
        {
            WriteLine(message);
            RuntimeError = true;
        }
        public CommandState(IOutput output)
        {
            _output = output;
        }
    }
}
