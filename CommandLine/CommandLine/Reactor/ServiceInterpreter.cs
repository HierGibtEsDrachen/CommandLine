using System;
using System.Collections.Generic;

namespace CommandLine.Reactor
{
    public class ServiceInterpreter
    {
        public static void RegisterDefaultErrors(MessageProvider provider)
        {
            if(provider != null)
            {
                provider.Register("service/argument/missing", "[{0}] is not an argument on [{1}].");
                provider.Register("service/missing", "[{0}] cant be found.");
                provider.Register("service/preexecute/failed", "pre execute for [{0}] failed.");
                provider.Register("service/failed", "[{0}] cant be executed.");
                provider.Register("service/success", "[{0}] finished.");
                provider.Register("service/executing", "[{0}] started.");
                provider.Register("service/name", "[{0}]");
                provider.Register("service/argument/missing", "[{0}] need arguments with parameter:");
                provider.Register("argument/missing", "[{0}] cant be found.");
                provider.Register("argument/duplicate", "[{0}] was already used.");
                provider.Register("argument/requiered/missing", "[{0}] is a requiered parameter but cant be found.");
                provider.Register("argument/parameter", "[{0}] -> [{1}]");
                provider.Register("argument/parameter/missing", "[{0}] need a parameter.");
                provider.Register("argument/parameter/empty", "[{0}] dont has a parameter.");
                provider.Register("argument/shortcut/notvalid", "[{0}] is not a valid shortcut.");
            }
        }
        internal ServiceMapCollection Services { get; }
        private IErrorLog _errorlog { get; }
        public ServiceInterpreter(ServiceMapCollection services, IErrorLog errorLog)
        {
            _errorlog = errorLog;
            Services = services;

        }
        public ServiceState Execute(ServiceOptions options, ICaller caller)
        {
            ServiceState state = new ServiceState(this, options, caller);
            Service service = Services.FindCreateByKey<Service>(options.Namespace, options.Name);
            if (state.RuntimeError = (service == null) && options.FullInformation)
                _errorlog.Pass(this, "service/missing", s => string.Format(s, options));
            else if (Execute(service, options, state) && state.Options.FullInformation)
                _errorlog.Pass(this, "service/success", s => string.Format(s, service));
            else if (state.Options.FullInformation)
                _errorlog.Pass(this, "service/failed", s => string.Format(s, service));
            return state;
        }
        private bool Execute(Service service, ServiceOptions options, ServiceState state)
        {
            if (state.Options.FullInformation)
            {
                _errorlog.Pass(this, "service/name", s => string.Format(s, options.Command));
                _errorlog.Pass(this, "service/name", s => string.Format(s, options));
            }
            try
            {
                service.Command.PreExecute.Invoke(state);
            }
            catch (Exception exception)
            {
                state.RuntimeError = true;
                if (state.Error && options.FullInformation)
                    _errorlog.Pass(this, "service/preexecute/failed", s => string.Format(s, service));
                state.WriteLine(exception.Message);
            }
            List<string> requiered = new List<string>(service.Command.GetRquieredArguments());
            List<string> listfoundargs = new List<string>();
            Action argexecutes = null;
            foreach (ArgumentInfo arg in options.Args)
            {
                if (state.Options.FullInformation)
                    _errorlog.Pass(this, "argument/parameter", s => string.Format(s, arg.Key, arg.Value));
                Argument<ServiceState> argument = null;
                if (arg.IsFullname) argument = service.Command.Find(arg.Key);
                else if (arg.Key.Length > 1)
                    _errorlog.Pass(this, "argument/shortcut/notvalid", s => string.Format(s, arg.Key));
                else argument = service.Command.Find(arg.Key[0]);
                if (argument == null)
                {
                    state.ParseError = true;
                    _errorlog.Pass(this, "service/argument/missing", s => string.Format(s, arg.Key, service));
                }
                else if(listfoundargs.Contains(arg.Key))
                {
                    state.RuntimeError = true;
                    _errorlog.Pass(this, "argument/duplicate", s => string.Format(s, arg.Key));
                }
                else if (string.IsNullOrWhiteSpace(arg.Value) && !argument.Empty)
                {
                    state.RuntimeError = true;
                    _errorlog.Pass(this, "argument/parameter/missing", s => string.Format(s, arg.Key));
                }
                else if(argument.Empty && !string.IsNullOrWhiteSpace(arg.Value))
                {
                    state.RuntimeError = true;
                    _errorlog.Pass(this, "argument/parameter/empty", s => string.Format(s, arg.Key));
                }
                else
                {
                    listfoundargs.Add(arg.Key);
                    argexecutes += () => argument.Action(state, arg.Value);
                }
                if (argument != null && argument.Requiered) requiered.Remove(argument.ToString());
            }
            if (requiered.Count > 0)
            {
                foreach (string arg in requiered)
                    _errorlog.Pass(this, "argument/requiered/missing", s => string.Format(s, arg));
                state.RuntimeError = true;
            }
            else if(state.Success)
            {
                try
                {
                    argexecutes?.Invoke();
                    if (state.Options.FullInformation)
                        _errorlog.Pass(this, "service/executing", s => string.Format(s, service.ToString()));
                    if (state.Success) service.Command.PostExecute.Invoke(state);
                }
                catch (Exception exception)
                {
                    state.RuntimeError = true;
                    state.WriteLine(exception.Message);
                }
            }
            return state.Success;
        }
    }
}
