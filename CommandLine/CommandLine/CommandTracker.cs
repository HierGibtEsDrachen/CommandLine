using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine
{
    public class CommandTracker
    {
        public IEnumerable<string> Commands { get { return _commands; } }
        List<string> _commands;
        int _index;
        int _count;
        public CommandTracker(int count)
        {
            _count = count;
            _commands = new List<string>(count);
        }
        public CommandTracker(int count, IEnumerable<string> commands)
        {
            _count = count;
            _commands = new List<string>(_count);
            int comcount = commands.Count();
            if (comcount > _count) _commands.AddRange(commands.Skip(comcount - _count));
            else _commands.AddRange(commands);
            _index = _commands.Count;
        }
        public string ScrollUp()
        {
            string command = string.Empty;
            if (_commands.Count > 0)
            {
                _index--;
                if (_index < 0) _index = 0;
                command = _commands[_index];
            }
            return command;
        }
        public string ScrollDown()
        {
            string command = string.Empty;
            if (_commands.Count > 0)
            {
                _index++;
                if (_index > _commands.Count) _index = _commands.Count;
                if (_index <= _commands.Count - 1) command = _commands[_index];
                else command = string.Empty;
            }
            return command;
        }
        public void Add(string command)
        {
            if (_commands.Count == _count) _commands.RemoveAt(0);
            _commands.Add(command);
            if (_commands.Count > _count) throw new InvalidProgramException("WTF IS WRONG WITH U");
            _index = _commands.Count;
        }
    }
}
