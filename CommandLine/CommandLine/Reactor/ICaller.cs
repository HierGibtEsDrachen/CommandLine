namespace CommandLine.Reactor
{
    public interface ICaller
    {
        string Name { get; }
        IOutput Output { get; }
    }
}