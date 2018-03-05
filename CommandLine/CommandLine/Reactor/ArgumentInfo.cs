using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine.Reactor
{
    public class ArgumentInfo
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsFullname { get; set; }
        public ArgumentInfo(string key, string value, bool isfullname)
        {
            Key = key;
            Value = value;
            IsFullname = isfullname;
        }
    }
}
