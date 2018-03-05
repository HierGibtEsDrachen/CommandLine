namespace CommandLine
{
    public interface IOutput
    {
        void WriteLine(string message);
        void Write(string message);
        void WriteLine();
    }
}