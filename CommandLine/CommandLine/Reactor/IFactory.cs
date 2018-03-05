using System;

namespace CommandLine.Reactor
{
    public interface IFactory<T>
    {
        T Create(Type type);
    }
}