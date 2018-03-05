using System;
using System.Text;

namespace CommandLine.Reactor
{
    public abstract class CommandWrapper<T>
    {
        public string Namespace { get; }
        public string Name { get; }
        internal Command<T> Command { get; }
        public CommandWrapper(string nameSpace, string name, string description)
        {
            Namespace = nameSpace.ToLower();
            Name = name.ToLower();
            Command = new Command<T>(Name, description, PostExecute, PreExecute);
        }
        public CommandWrapper(string nameSpace, string name) : this(nameSpace, name, "")
        {
        }
        protected void Register(Argument<T> argument)
        {
            Command.Add(argument);
        }
        protected void Register(string name, char shortcut, bool requiered, bool emptyparameter, Action<T, string> action)
        {
            Command.Add(new Argument<T>(name, shortcut, requiered, emptyparameter, action));
        }
        protected abstract void Execute(T state);
        protected virtual void PreExecute(T state)
        {
        }
        protected virtual void OnExecuted(T state)
        {
        }
        private void PostExecute(T state)
        {
            Execute(state);
            OnExecuted(state);
        }
        public string PrintFullInformation()
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Namespace))
                builder.Append($"{Namespace}:" + Command.PrintFullInformation());
            else builder.Append(Command.PrintFullInformation());
            foreach(Argument<T> arg in Command.Arguments)
            {
                builder.AppendLine(arg.PrintFullInformation());
            }
            return builder.ToString();
        }
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Namespace)) return Name;
            return $"{Namespace}:{Name}";
        }
    }
}
