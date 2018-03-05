using CommandLine.Reactor;
using System;
using System.Collections.Generic;
using System.Text;
namespace CommandLine.Initializer
{
    public class NestedArgument : Argument
    {
        public static int ResolveNestedParenthesses(string input, char entrychar, char escapechar, out IEnumerable<NestedArgument> result)
        {
            List<NestedArgument> args = new List<NestedArgument>();
            IEnumerable<ResolvedText> resolved;
            if (ResolvedText.ResolveNestedParenthesses(input, entrychar, escapechar, out resolved) > 0)
            {
                foreach (ResolvedText text in resolved) args.Add(ResolveNode(text, entrychar, escapechar, false));
            }
            result = args;
            return args.Count;
        }
        public static NestedArgument ResolveNode(ResolvedText text, char entrychar, char escapechar, bool isnested)
        {
            Dictionary<string, Argument> arguments = new Dictionary<string, Argument>();
            StringParser parser = new StringParser(text.Content);
            parser.IgnoreRead(' ');
            string scopekey = parser.SkipIf(entrychar).SkipIf(' ').ReadUntilAndSkip(' ');
            while (!parser.End)
            {
                Argument resarg;
                string val;
                string key = parser.SkipIf(' ').ReadUntil('=', ',');
                if (parser.Current == '=')
                {
                    val = parser.SkipIf('=').ReadUntilAndSkip(',');
                    if (val[0] == entrychar && val[val.Length - 1] == escapechar && text.NestedText.Count > 0)
                    {
                        int nodeindex = int.Parse(val.Substring(1, val.Length - 2));
                        resarg = ResolveNode(text.NestedText[nodeindex], entrychar, escapechar, true);
                    }
                    else resarg = new Argument(val);
                }
                else if (parser.Current == ',')
                {
                    val = key;
                    key = string.Empty;
                    resarg = new Argument(val);
                    parser.SkipIf(',');
                }
                else
                {
                    val = key;
                    key = string.Empty;
                    resarg = new Argument(val);
                }
                arguments.Add(key, resarg);
            }
            return new NestedArgument(scopekey, text.Content, isnested, arguments);
        }
        public string Scope { get; }
        public IReadOnlyDictionary<string, Argument> KeyValue { get; }
        internal NestedArgument(string scope, string value, bool isnedted, Dictionary<string, Argument> keyedvalues) : base(value, isnedted)
        {
            Scope = scope;
            KeyValue = keyedvalues;
        }
        public Argument FindArgument(string name)
        {
            if (KeyValue.TryGetValue(name, out Argument result)) return result;
            return null;
        }
    }
    public class Argument
    {
        public bool IsNested { get; }
        public string Value { get; }
        public Argument(string value) : this(value, false)
        {

        }
        internal Argument(string value, bool nested)
        {
            IsNested = nested;
            Value = value;
        }
        public override string ToString()
        {
            return Value;
        }
    }
    public class ResolvedText
    {
        public static int ResolveNestedParenthesses(string input, char entrychar, char escapechar, out IEnumerable<ResolvedText> result)
        {
            List<ResolvedText> resolved = new List<ResolvedText>();
            Stack<ResolvedText> stack = new Stack<ResolvedText>();
            StringBuilder builder = new StringBuilder();
            ResolvedText current = null;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == entrychar)
                {
                    if (current != null)
                    {
                        current.Content = builder.Append(entrychar).Append(current.NestedText.Count).Append(escapechar).ToString();
                        stack.Push(current);
                        current = new ResolvedText();
                    }
                    else resolved.Add(current = new ResolvedText());
                    if (stack.Count > 0) stack.Peek().NestedText.Add(current);
                    current.Start = Math.Min(i, input.Length);
                    builder.Clear();
                }
                else if (input[i] == escapechar)
                {
                    if (current == null) throw new InvalidProgramException();
                    current.End = Math.Max(i, 0);
                    current.Content = builder.ToString();
                    builder.Clear();
                    if (stack.Count > 0)
                    {
                        current = stack.Pop();
                        builder.Append(current.Content);
                    }
                    else current = null;
                }
                else builder.Append(input[i]);
            }
            if (stack.Count > 0) throw new InvalidProgramException();
            result = resolved;
            return resolved.Count;
        }
        public List<ResolvedText> NestedText { get; }
        public bool IsValid { get { return Start < End; } }
        public int Start { get; set; }
        public int End { get; set; }
        public string Content { get; set; }
        public ResolvedText()
        {
            NestedText = new List<ResolvedText>();
        }
    }
}
