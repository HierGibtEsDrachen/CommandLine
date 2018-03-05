using Extensions;
using System;
using System.Linq;
using System.Text;

namespace CommandLine.Reactor
{
    public class Argument<T>
    {
        public string Name { get; }
        public char Shortcut { get; }
        public bool Requiered { get; }
        public bool Empty { get; }
        public string Description { get; set; }
        public Action<T, string> Action { get; }
        public Argument(string name, char shortcut, bool requiered, bool emptyparameter, Action<T, string> action)
        {
            Action = action ?? throw new ArgumentNullException("Action");
            Name = name.ToLower();
            Requiered = requiered;
            Empty = emptyparameter;
            Shortcut = shortcut.ToString().ToLower().First();
        }
        public Argument(string name, char shortcut, bool requiered, bool emptyparameter, string description, Action<T, string> action) : 
            this(name, shortcut, requiered, emptyparameter, action)
        {
            Description = description;
        }
        public override string ToString()
        {
            if (Name.NullOrEmpty()) return $"{Shortcut}";
            return $"{Name}|{Shortcut}";
        }
        public string PrintFullInformation()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(ToString());
            if (Requiered) builder.Append("\t\t Requiered").AppendLine();
            if(Description.NotNullOrEmpty()) builder.AppendLine(Description);
            return builder.ToString();
        }
    }
    public class InputArgument<T>
    {
        public string Name { get; }
        public string Parameter { get; set; }
        public bool IsFullname { get; }
        public Argument<T> Attached { get; set; }
        public InputArgument(string key, bool fullname)
        {
            Name = key;
            IsFullname = fullname;
        }
        public InputArgument(string name, string parameter, bool fullname) : this(name, fullname)
        {
            Parameter = parameter;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
