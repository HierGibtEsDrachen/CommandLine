using System;
namespace CommandLine.Reactor
{
    public class ServiceState : CommandState
    {
        public ServiceOptions Options { get; }
        public ICaller Caller { get; }
        private ServiceInterpreter _executer;
        public ServiceState(ServiceInterpreter executer, ServiceOptions options, ICaller caller) : base(caller.Output)
        {
            Options = options;
            _executer = executer;
            Caller = caller;
        }
        public T Find<T>(string nameSpace, string key) where T : Service
        {
            return _executer.Services.FindCreateByKey<T>(nameSpace, key);
        }
        public ServiceState Execute(string command, int id, bool fullinformation = false)
        {
            ServiceOptions options = ServiceOptions.Parse(command, id, Options.FullInformation || fullinformation);
            if (options == null)
            {
                ServiceState state = new ServiceState(_executer, options, Caller);
                if(fullinformation) state.WriteLine("cant parse given command: " + command);
                return state;
            }
            else return _executer.Execute(options, Caller);
        }
    }
}
