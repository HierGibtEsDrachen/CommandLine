using System;
using System.Collections.Generic;
using System.Linq;
namespace CommandLine.Reactor
{
    public class CommandExecuter
    {
        private readonly char _space = ' ';
        private readonly char _mark = '-';
        private readonly char _equal = '=';
        static readonly Dictionary<string, string> CommandMessagPack = new Dictionary<string, string>()
        {
            {"Arg/Name", "[{0}|{1}]"},
            {"Arg/Missing", "[{0}] konnte nicht gefuden werden."},
            {"Arg/Parameter/Empty", "[{0}] hat keine Parameter." },
            {"Arg/Parameter", "[{0}]->[{1}]"},
            {"Arg/Parameter/None", "[{0}] akzeptiert keine Parameter." },
            {"Arg/Parameter/Missing", "[{0}] benötigt einen Parameter." },
            {"Arg/Missing/InList", "[{0}] ist ein benötigtes Argument, dass in der Liste nicht augetreten ist." },
            {"Command/Name", "[{0}]" },
            {"Command/Missing", "[{0}] konnte nicht gefunden werden."},
            {"Command/Args/Missing", "[{0}] benötigt folgenden Argumente:"},
            {"Command/NoArgs", "[{0}] nimmt keine Argumente entgegen."},
            {"Command/Skipping", "[{0}] kann nicht ausgeführt werden." },
            {"Command/Executing", "[{0}] wird ausgeführt." },
            {"Command/Executed", "[{0}] ausgefürht." },
            {"Command/Failed", "[{0}] konnte nicht ausgeführt werden." },
        };
        public bool Interpret(string command, Register<Command<CommandState>> set, IOutput output, out CommandState state)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (set == null) throw new ArgumentNullException(nameof(set));
            state = new CommandState(output);
            InputCommand<CommandState> inputcommand = Parse(command);
            Command<CommandState> commandwrapper = set.Find(inputcommand.Name);
            if (commandwrapper == null)
            {
                state.WriteLine(string.Format(CommandMessagPack["Command/Missing"], commandwrapper.Name));
                state.ParseError = true;
                return state.Success;
            }
            return Execute(inputcommand, commandwrapper, state);
        }
        public bool Interpret(string command, Register<CommandWrapper<CommandState>> set, IOutput output, out CommandState state)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (set == null) throw new ArgumentNullException(nameof(set));
            state = new CommandState(output);
            InputCommand<CommandState> inputcommand = Parse(command);
            CommandWrapper<CommandState> commandwrapper = set.Find(inputcommand.Name);
            if (commandwrapper == null)
            {
                state.WriteLine(string.Format(CommandMessagPack["Command/Missing"], commandwrapper.Name));
                state.ParseError = true;
                return state.Success;
            }
            return Execute(inputcommand, commandwrapper.Command, state);
        }
        public bool Interpret(InputCommand<CommandState> inputcommand, Register<Command<CommandState>> set, IOutput output, out CommandState state)
        {
            if (inputcommand == null) throw new ArgumentNullException(nameof(inputcommand));
            if (set == null) throw new ArgumentNullException(nameof(set));
            state = new CommandState(output);
            Command<CommandState> commandwrapper = set.Find(inputcommand.Name);
            if (commandwrapper == null)
            {
                state.WriteLine(string.Format(CommandMessagPack["Command/Missing"], commandwrapper.Name));
                state.ParseError = true;
                return state.Success;
            }
            else return Execute(inputcommand, commandwrapper, state);
        }
        public bool Interpret(InputCommand<CommandState> inputcommand, Register<CommandWrapper<CommandState>> set, IOutput output, out CommandState state)
        {
            if (inputcommand == null) throw new ArgumentNullException(nameof(inputcommand));
            if (set == null) throw new ArgumentNullException(nameof(set));
            state = new CommandState(output);
            CommandWrapper<CommandState> commandwrapper = set.Find(inputcommand.Name);
            if (commandwrapper == null)
            {
                state.WriteLine(string.Format(CommandMessagPack["Command/Missing"], commandwrapper.Name));
                state.ParseError = true;
            }
            else return Execute(inputcommand, commandwrapper.Command, state);
            return state.Success;
        }
        public bool Execute(InputCommand<CommandState> inputcommand, Command<CommandState> command, CommandState state)
        {
            if (inputcommand == null) throw new ArgumentNullException(nameof(inputcommand));
            if (command == null) throw new ArgumentNullException(nameof(command));
            state.WriteLine(string.Format(CommandMessagPack["Command/Name"], command.Name));
            try
            {
                command.PreExecute?.Invoke(state);
            }
            catch(Exception exception)
            {
                state.RuntimeError = true;
                state.WriteLine(exception.Message);
            }
            if (state.Error)
            {
                state.WriteLine(string.Format(CommandMessagPack["Command/Skipping"], command.Name));
            }
            else if (command.TotalArguments == 0 && inputcommand.Args.Count > 0)
                state.WriteLine(string.Format(CommandMessagPack["Command/NoArgs"], command.Name));
            else SearchArguments(inputcommand, command, state);         
            if(state.Success)
            {
                try
                {
                    state.WriteLine(string.Format(CommandMessagPack["Command/Executing"], command.Name));
                    foreach (InputArgument<CommandState> args in inputcommand.Args)
                    {
                        args.Attached?.Action.Invoke(state, args.Parameter);
                        if (state.Error) break;
                    }
                    if (state.Success) command.PostExecute.Invoke(state);
                }
                catch (Exception exception)
                {
                    state.RuntimeError = true;
                    state.WriteLine(exception.Message);
                }
                if (state.Error) state.WriteLine(string.Format(CommandMessagPack["Command/Failed"], command.Name));
                else state.WriteLine(string.Format(CommandMessagPack["Command/Executed"], command.Name));
            }
            return state.Success;
        }
        public InputCommand<CommandState> Parse(string command)
        {
            StringParser parser = new StringParser(command);
            string commandname = parser.SkipIf(_space).ReadUntil(_space, _mark);
            InputCommand<CommandState> inputcommand;
            if (commandname.Contains(":"))
            {
                string[] splittedname = commandname.Split(':');
                string nameSpace = splittedname[0];
                string name = splittedname[1];
                inputcommand = new InputCommand<CommandState>(nameSpace, name);
            }
            else inputcommand = new InputCommand<CommandState>("", commandname);
            while (!parser.End)
            {
                parser.SkipUntil(_mark);
                if (parser.Peek(1) == _mark)
                {
                    InputArgument<CommandState> argument = new InputArgument<CommandState>(parser.SkipIf(_mark).ReadUntil(_equal, _mark), true);
                    inputcommand.Args.Add(argument);
                    if (parser.Peek() == _equal)
                    {
                        argument.Parameter = parser.Skip(_equal, 1).ReadUntilNext(_mark);
                        if (parser.Peek() != _space) argument.Parameter += parser.ReadUntil(_mark);
                    }
                }
                else
                {
                    InputArgument<CommandState> argument = new InputArgument<CommandState>(parser.SkipIf(_mark).ReadUntil(_space, _mark).ToLower(), false);
                    inputcommand.Args.Add(argument);
                    argument.Parameter = parser.SkipIf(_space).ReadUntilNext(_mark);
                    if (parser.Peek() != _space) argument.Parameter += parser.ReadUntil(_mark);
                }
            }
            return inputcommand;
        }
        private void SearchArguments(InputCommand<CommandState> inputcommand, Command<CommandState> command, CommandState state)
        {
            List<string> requiered = new List<string>(command.GetRquieredArguments());
            foreach (InputArgument<CommandState> arg in inputcommand.Args)
            {
                state.WriteLine(string.Format(CommandMessagPack["Arg/Name"], arg.Name));
                Argument<CommandState> argument = null;
                if (arg.IsFullname) argument = command.Find(arg.Name);
                else argument = command.Find(arg.Name[0]);
                if (argument == null)
                {
                    state.WriteLine(string.Format(CommandMessagPack["Arg/Missing"], arg.Name));
                    state.ParseError = true;
                }
                else if (string.IsNullOrEmpty(arg.Parameter) && !argument.Empty)
                {
                    state.WriteLine(string.Format(CommandMessagPack["Arg/Parameter/Missing"], argument.Name));
                    state.ParseError = true;
                }
                else if(argument.Empty)
                {
                    state.WriteLine(string.Format(CommandMessagPack["Arg/Parameter/none"], arg.Name));
                    state.ParseError = true;
                }
                else
                {
                    state.WriteLine(string.Format(CommandMessagPack["Arg/Parameter"], arg.Name, arg.Parameter));
                    arg.Attached = argument;
                    if (argument.Requiered && !requiered.Remove(argument.Name))
                    {
                        state.WriteLine(string.Format(CommandMessagPack["Arg/Missing/InList"], argument.Name));
                        state.ParseError = true;
                    }
                }
            }
            if (requiered.Count > 0)
            {
                state.WriteLine(string.Format(CommandMessagPack["Command/Args/Missing"], command.Name));
                foreach (string missingarg in requiered)
                    state.WriteLine(string.Format("[{0}]", missingarg));
                state.ParseError = true;
            }
        }
        
    }
}
