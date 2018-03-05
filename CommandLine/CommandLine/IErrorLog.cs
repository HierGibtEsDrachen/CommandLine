using System;

namespace CommandLine
{
    public interface IErrorLog : IOutput
    {
        void Pass(object sender, string key, Func<string, string> wrapper);
        void Pass(object sender, string key);
    }
}