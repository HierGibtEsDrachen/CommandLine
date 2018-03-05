using System;
using System.Collections.Generic;
namespace CommandLine.Reactor
{
    public class MessageProvider
    {
        private Dictionary<string, string> _messagedictionary;
        public MessageProvider()
        {
            _messagedictionary = new Dictionary<string, string>();
        }
        public string Search(string key)
        {
            if (_messagedictionary.TryGetValue(key, out string message)) return message;
            else return $"key: [{key}] not found";
        }
        public string Search(string key, Func<string, string> formater)
        {
            return formater(Search(key));
        }
        public bool Register(string key, string content)
        {
            if (_messagedictionary.ContainsKey(key)) return false;
            else _messagedictionary.Add(key, content);
            return true;
        }
    }
}
