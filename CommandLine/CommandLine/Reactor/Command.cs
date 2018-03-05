using Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLine.Reactor
{
    public class Command<T>
    {
        public string Name { get; }
        public bool HasRequieredArguments { get { return _requieredArguments.Count > 0; } }
        public int TotalArguments { get { return _nameSorted.Count; } }
        public string Description { get; set; }
        public IEnumerable<Argument<T>> Arguments { get { return _nameSorted.Values; } }
        public Action<T> PreExecute { get; }
        public Action<T> PostExecute { get; }
        Dictionary<char, Argument<T>> _shortcutSorted;
        Dictionary<string, Argument<T>> _nameSorted;
        List<string> _requieredArguments;
        public Command(string name, string description, Action<T> postExecute, 
            Action<T> preExecute) : this(name, postExecute)
        {
            PreExecute = preExecute;
            Description = description;
        }
        public Command(string name, string description, Action<T> postExecute) : this(name, postExecute)
        {
            Description = Description;
        }
        public Command(string name, Action<T> postExecute)
        {
            _requieredArguments = new List<string>();
            _shortcutSorted = new Dictionary<char, Argument<T>>();
            _nameSorted = new Dictionary<string, Argument<T>>();
            Name = name.ToLower();
            PostExecute = postExecute ?? throw new ArgumentNullException("PostExecute");
        }
        public string PrintFullInformation()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(ToString());
            if (Description.NotNullOrEmpty()) builder.AppendLine(Description);
            return builder.ToString(); 
        }
        public void Add(Argument<T> argument)
        {
            if (argument.Name.NullOrEmpty()) throw new InvalidProgramException();
            if (_shortcutSorted.ContainsKey(argument.Shortcut))
                throw new ArgumentException(string.Format("Das Argument mit dem Shortcut [{0}] ist bereits enthalten.", argument.Shortcut));
            else _shortcutSorted.Add(argument.Shortcut, argument);
            if (_nameSorted.ContainsKey(argument.Name))
                throw new ArgumentException(string.Format("Das Argument mit dem Namen [{0}] ist bereits enthalten.", argument.Name));
            else _nameSorted.Add(argument.Name, argument);
            if (argument.Requiered)
                _requieredArguments.Add(argument.ToString());
        }
        public bool Remove(string name)
        {
            if (_nameSorted.TryGetValue(name, out Argument<T> arg))
            {
                _nameSorted.Remove(arg.Name);                
                return _shortcutSorted.Remove(arg.Shortcut);
            }
            return false;
        }
        public bool Remove(char shortcut)
        {
            if (_shortcutSorted.TryGetValue(shortcut, out Argument<T> arg))
            {                
                _nameSorted.Remove(arg.Name);
                return _shortcutSorted.Remove(arg.Shortcut);
            }
            return false;
        }
        public IEnumerable<string> GetRquieredArguments()
        {
            return _requieredArguments;
        }
        internal Argument<T> Find(char shortcut)
        {
            if (_shortcutSorted.TryGetValue(shortcut, out Argument<T> arg))
                return arg;
            return null;
        }
        internal Argument<T> Find(string key)
        {
            if (_nameSorted.TryGetValue(key, out Argument<T> arg))
                return arg;
            return null;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class InputCommand<T>
    {
        public string Namespace { get; }
        public string Name { get; }
        public ICollection<InputArgument<T>> Args { get; }
        public InputCommand(string nameSpace, string name)
        {
            Namespace = nameSpace;
            Name = name;
            Args = new List<InputArgument<T>>();
        }
        public override string ToString()
        {
            if (Namespace.NullOrEmpty()) return Name;
            return $"{Namespace}:{Name}";
        }
    }
}
