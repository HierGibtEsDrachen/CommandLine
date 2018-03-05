namespace CommandLine
{
    public interface ISource
    {
        string Name { get; }
        bool TryGetResource(string key, out object obj);
    }
}