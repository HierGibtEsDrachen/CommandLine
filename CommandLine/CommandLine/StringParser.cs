using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace CommandLine
{
    public class StringParser
    {
        public char Current { get { return Peek(); } }
        public char Next { get { return Peek(1); } }
        public int CharsToRead { get { return Lenght - Position; } }
        public int Position { get { return _position; } }
        public int Lenght { get { return _input.Length; } }
        public char EndOfText { get { return _endoftext; } }
        public bool End { get { return _position >= _input.Length; } }
        private string _input;
        private int _position;
        private int _reversedposition;
        private StringBuilder _intenalBuilder;
        private List<char> _charstoignor;
        private const char _endoftext = '\0';
        public StringParser(string input)
        {
            _input = (string)input.Clone();
            _intenalBuilder = new StringBuilder();
            _charstoignor = new List<char>();
            _reversedposition = _input.Length - 1;
        }
        /// <summary>
        /// List ausnahmslos einen <see cref="char"/> aus dem <see cref="string"/> und schiebt den Pointer um eine Position weiter.
        /// </summary>
        /// <returns></returns>
        public char Read()
        {
            if (!End)
            {
                char result = _input[_position];
                _position++;
                return result;
            }
            else return _endoftext;
        }
        public string Read(int count)
        {
            if (_position + count > _input.Length)
                count = _input.Length - _position;
            return RemoveIgnoredChars(_input.Substring(_position, count));
        }
        public string ReadUntil(char mark)
        {
            _intenalBuilder.Clear();
            while (!End && Peek() != mark)
            {
                _intenalBuilder.Append(Read());
            }
            return RemoveIgnoredChars(_intenalBuilder.ToString());
        }
        public string ReadUntilAndSkip(char mark)
        {
            string result = ReadUntil(mark);
            SkipIf(mark);
            return RemoveIgnoredChars(result);
        }
        public string ReadUntilBreakAndSkip(char mark, params char[] breakif)
        {
            string result = ReadUntil(mark, breakif);
            if(Peek() == mark) SkipIf(mark);
            return RemoveIgnoredChars(result);
        }
        public string ReadUntil(char mark, params char[] breakif)
        {
            _intenalBuilder.Clear();
            char current;
            char readed;
            while (!End && (current = Peek()) != mark && !CompareToBreak(breakif, current))
            {
                readed = Read();
                if (CompareIgnore(readed)) continue;
                _intenalBuilder.Append(readed);
            }
            return _intenalBuilder.ToString();
        }
        public StringParser SkipUntil(char mark, bool breakonnotempychars)
        {
            while (!End && _input[_position] != mark)
            {
                if (_input[_position] != ' ') break;
                _position++;
            }
            return this;
        }
        public string ReadUntil(char mark, bool reverse)
        {
            char current;
            while(_reversedposition > _position && (current = _input[_reversedposition]) != mark)
            {
                _reversedposition--;
            }
            return Read(_reversedposition - _position + 1);
        }
        public string ReadUntilInclude(char mark)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(ReadUntil(mark));
            builder.Append(Read());
            return builder.ToString();
        }
        public bool ReadUntil(char mark, out string value, params char[] breakif)
        {
            _intenalBuilder.Clear();
            char current;
            bool succed = true;
            while (!End && (current = Peek()) != mark)
            {
                if (CompareIgnore(current)) continue;
                if (CompareToBreak(breakif, current))
                {
                    succed = false;
                    break;
                }
                _intenalBuilder.Append(Read());
            }
            value = _intenalBuilder.ToString();
            return succed;
        }
        public string ReadUntilNext(char mark)
        {
            _intenalBuilder.Clear();
            if (Peek() == mark) return _intenalBuilder.ToString();
            while (!End && Peek(1) != mark)
            {
                _intenalBuilder.Append(Read());
            }
            return RemoveIgnoredChars(_intenalBuilder.ToString());
        }
        public string ReadUntilNext(char mark, params char[] breakif)
        {
            _intenalBuilder.Clear();
            if (Peek() == mark) return _intenalBuilder.ToString();
            char next;
            while (!End && (next = Peek(1)) != mark)
            {
                CompareIgnore(Peek());
                _intenalBuilder.Append(Read());
            }
            return RemoveIgnoredChars(_intenalBuilder.ToString());
        }
        public string ReadToEnd()
        {
            string result = RemoveIgnoredChars(_input.Substring(_position, _input.Length - _position));
            _position = _input.Length - 1;
            return result;
        }
        public string ReadToEnd(params char[] ingore)
        {
            foreach(char igno in ingore) IgnoreRead(igno);
            string result = RemoveIgnoredChars(_input.Substring(_position, _input.Length - _position));
            _position = _input.Length - 1;
            foreach (char igno in ingore) AllowRead(igno);
            return result;
        }
        public char PeekLast()
        {
            if (_position > 0) return _input[_position - 1];
            else return _endoftext;
        }
        public char Peek()
        {
            if (!End) return _input[_position];
            else return _endoftext;
        }
        public char Peek(int offset)
        {
            if (End) return _endoftext;
            int index = _position + offset;
            if (index < 0) return _input[0];
            else if(index < _input.Length) return _input[index];
            else return _endoftext;
        }
        public StringParser Skip(int count)
        {
            if (End) return this;
            _position += count;
            if (_position > _input.Length)
                _position = _input.Length;
            return this;
        }
        public StringParser Skip(char mark, int count)
        {
            if (End) return this;
            int skippedCount = 0;
            while (!End && count > skippedCount && _input[_position] == mark)
            {
                _position++;
                skippedCount++;
            }
            return this;
        }
        public StringParser SkipIf(char mark)
        {
            while (!End && _input[_position] == mark)
                _position++;
            return this;
        }
        public StringParser SkipUntil(char mark)
        {
            while (!End && _input[_position] != mark)
                _position++;
            return this;
        }
        public StringParser SkipUntil(char mark, char breakif)
        {
            char current;
            while (!End && (current = Read()) != mark)
            {
                if (current == breakif) break;
            }
            return this;
        }
        public StringParser Rest()
        {
            _position = 0;
            return this;
        }
        public void IgnoreRead(char chartoignore)
        {
            if (!_charstoignor.Contains(chartoignore))
                _charstoignor.Add(chartoignore);
        }
        public void AllowRead(char charrtoread)
        {
            _charstoignor.Remove(charrtoread);
        }
        private string RemoveIgnoredChars(string input)
        {
            if (_charstoignor.Count == 0) return input;
            string result = input;
            foreach (char ignorechar in _charstoignor)
            {
                result = result.Replace(ignorechar.ToString(), string.Empty);
            }
            return result;
        }
        private bool CompareToBreak(char[] breakif, char current)
        {
            if (breakif.Contains(current)) return true;
            return false;
        }
        private bool CompareIgnore(char current)
        {
            if (_charstoignor.Contains(current)) return true;
            return false;
        }
    }
}
