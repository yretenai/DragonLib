using System.Reflection;

namespace DragonLib.CommandLine;

public static class Command {
    //                        group,             name
    private static Dictionary<string, Dictionary<string, (string Description, Type Type, Type Command)>> Commands { get; } = new() {
        { string.Empty, new Dictionary<string, (string Description, Type Type, Type Command)>() },
    };

    private static void LoadCommands() {
        var types = Assembly.GetEntryAssembly()?.GetTypes() ?? Type.EmptyTypes;
        foreach (var type in types) {
            var commandAttributes = type.GetCustomAttributes<CommandAttribute>();
            foreach (var commandAttribute in commandAttributes) {
                var flagsType = commandAttribute.FlagsType;
                if (!Commands.ContainsKey(commandAttribute.Group)) {
                    Commands[commandAttribute.Group] = new Dictionary<string, (string Description, Type Type, Type Command)>();
                }

                Commands[commandAttribute.Group][commandAttribute.Name] = (commandAttribute.Description, flagsType, type);
            }
        }
    }

    public static void Run(CommandLineFlags? globalFlags = null, CommandLineFlagsParser.PrintHelpDelegate? printHelp = null,  string[]? args = null) {
        LoadCommands();

        args ??= Environment.GetCommandLineArgs().Skip(1).ToArray();
        printHelp ??= CommandLineFlagsParser.PrintHelp;
        var positionalFlags = CommandLineFlagsParser.ParseFlags<CommandLineFlags>(new CommandLineOptions { UseHelp = false });

        if (positionalFlags == null) {
            return;
        }

        globalFlags ??= positionalFlags;

        var commandGroupName = positionalFlags.Positionals.ElementAtOrDefault(0);
        if (string.IsNullOrEmpty(commandGroupName)) {
            Console.WriteLine("No command specified, available commands:");
            foreach(var (group, commands) in Commands) {
                if (!string.IsNullOrEmpty(group)) {
                    Console.WriteLine(group);
                }

                foreach(var (name, (description, _, _)) in commands) {
                    Console.WriteLine($"{(string.IsNullOrEmpty(group) ? string.Empty : "  ")}{name} - {description}");
                }
            }

            return;
        }

        Dictionary<string, (string Description, Type Type, Type Command)>? commandGroup;

        var commandName = positionalFlags.Positionals.ElementAtOrDefault(1);
        if (string.IsNullOrEmpty(commandName) || !Commands.ContainsKey(commandGroupName)) {
            if (Commands.TryGetValue(commandGroupName, out commandGroup)) {
                Console.WriteLine("No command specified, available commands:");
                foreach(var (name, (description, _, _)) in commandGroup) {
                    Console.WriteLine($"{name} - {description}");
                }

                return;
            }

            commandName = commandGroupName;
            commandGroupName = string.Empty;
        }

        commandGroup = Commands[commandGroupName];

        if (!commandGroup.TryGetValue(commandName, out var command)) {
            Console.WriteLine($"Command {commandName} not found");
            return;
        }

        var offset = string.IsNullOrEmpty(commandGroupName) ? 1 : 2;
        var filteredArgs = args.Where(x => x[0] == '-').Concat(positionalFlags.Positionals.Skip(offset)).ToArray();

        var flags = CommandLineFlagsParser.ParseFlags(command.Type, printHelp, new CommandLineOptions { Command = $"{commandGroupName} {commandName}".Trim(), PositionalOffset = offset }, filteredArgs);
        if (flags == null) {
            return;
        }

        var constructors = command.Command.GetConstructors();
        var globalType = globalFlags.GetType();
        var globalAndFlagsConstructor = constructors.FirstOrDefault(x => {
            var @params = x.GetParameters();
            return @params.Length == 2 && @params[0].ParameterType.IsAssignableFrom(globalType) && @params[1].ParameterType.IsAssignableFrom(command.Type);
        });

        if (globalAndFlagsConstructor != null) {
            globalAndFlagsConstructor.Invoke(new object[] { globalFlags, flags });
            return;
        }

        var flagsConstructor = constructors.FirstOrDefault(x => {
            var @params = x.GetParameters();
            return @params.Length == 1 && @params[0].ParameterType.IsAssignableFrom(command.Type);
        });

        if (flagsConstructor != null) {
            flagsConstructor.Invoke(new object[] { flags });
            return;
        }

        Console.WriteLine($"Command {commandName} is not configured properly. Lacking a valid constructor.");
    }
}
