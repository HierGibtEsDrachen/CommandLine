namespace CommandLine.Reactor
{
    public abstract class Service : CommandWrapper<ServiceState>
    {
        public Service(string nameSpace, string name) : base(nameSpace, name)
        {

        }
        public sealed override string ToString()
        {
            return string.IsNullOrWhiteSpace(Namespace) ? Name : $"{Namespace}:{Name}";
        }
    }
}
