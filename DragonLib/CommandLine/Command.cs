using System.Reflection;

namespace DragonLib.CommandLine;

public static class Command {
    //                        group,             name
    public static Dictionary<string, Dictionary<string, (string Description, Type Type, Type Command, bool Hide)>> Commands { get; } = new() {
        { string.Empty, new Dictionary<string, (string Description, Type Type, Type Command, bool Hide)>() },
    };

    private static void LoadCommands() {
        var types = Assembly.GetEntryAssembly()?.GetTypes() ?? Type.EmptyTypes;
        foreach (var type in types) {
            var commandAttributes = type.GetCustomAttributes<CommandAttribute>();
            foreach (var commandAttribute in commandAttributes) {
                var flagsType = commandAttribute.FlagsType;
                if (!Commands.ContainsKey(commandAttribute.Group)) {
                    Commands[commandAttribute.Group] = new Dictionary<string, (string Description, Type Type, Type Command, bool)>();
                }

                Commands[commandAttribute.Group][commandAttribute.Name] = (commandAttribute.Description, flagsType, type, commandAttribute.Hide);
            }
        }
    }

    public static void Run(out string? commandName, out string? commandGroupName, CommandLineFlags? globalFlags = null, CommandLineFlagsParser.PrintHelpDelegate? printHelp = null, object[]? carry = null, string[]? args = null) => Run<object>(out commandName, out commandGroupName, globalFlags, printHelp, carry, args);

    public static T? Run<T>(out string? commandName, out string? commandGroupName, CommandLineFlags? globalFlags = null, CommandLineFlagsParser.PrintHelpDelegate? printHelp = null, object[]? carry = null, string[]? args = null) {
        LoadCommands();

        commandName = null;
        commandGroupName = null;

        args ??= Environment.GetCommandLineArgs().Skip(1).ToArray();
        printHelp ??= CommandLineFlagsParser.PrintHelp;
        var positionalFlags = CommandLineFlagsParser.ParseFlags<CommandLineFlags>(new CommandLineOptions { UseHelp = false });

        if (positionalFlags == null) {
            return default;
        }

        globalFlags ??= positionalFlags;

        commandGroupName = positionalFlags.Positionals.ElementAtOrDefault(0);
        if (string.IsNullOrEmpty(commandGroupName)) {
            Console.WriteLine("No command specified, available commands:");
            foreach (var (group, commands) in Commands) {
                if (commands.All(x => x.Value.Hide)) {
                    continue;
                }

                if (!string.IsNullOrEmpty(group)) {
                    Console.WriteLine(group);
                }

                foreach (var (name, (description, _, _, _)) in commands.Where(x => !x.Value.Hide)) {
                    Console.WriteLine($"{(string.IsNullOrEmpty(group) ? string.Empty : "  ")}{name} - {description}");
                }
            }

            return default;
        }

        Dictionary<string, (string Description, Type Type, Type Command, bool Hide)>? commandGroup;

        commandName = positionalFlags.Positionals.ElementAtOrDefault(1);
        if (string.IsNullOrEmpty(commandName) || !Commands.ContainsKey(commandGroupName)) {
            if (Commands.TryGetValue(commandGroupName, out commandGroup)) {
                if (commandGroup.All(x => x.Value.Hide)) {
                    Console.WriteLine("No command specified.");
                    return default;
                }

                Console.WriteLine("No command specified, available commands:");
                foreach (var (name, (description, _, _, _)) in commandGroup.Where(x => !x.Value.Hide)) {
                    Console.WriteLine($"{name} - {description}");
                }

                return default;
            }

            commandName = commandGroupName;
            commandGroupName = string.Empty;
        }

        commandGroup = Commands[commandGroupName];

        if (!commandGroup.TryGetValue(commandName, out var command)) {
            Console.WriteLine($"Command {commandName} not found");
            return default;
        }

        var offset = string.IsNullOrEmpty(commandGroupName) ? 1 : 2;

        var flags = command.Type == globalFlags.GetType() ? globalFlags : CommandLineFlagsParser.ParseFlags(command.Type, printHelp, new CommandLineOptions { Command = $"{commandGroupName} {commandName}".Trim(), SkipPositionals = offset }, args);
        if (flags == null) {
            return default;
        }

        var stack = new object[2 + (carry?.Length ?? 0)];
        stack[0] = globalFlags;
        stack[1] = flags;
        if (carry != null) {
            Array.Copy(carry, 0, stack, 2, carry.Length);
        }

        var stackTypes = stack.Select(x => x.GetType()).ToArray();

        // all types + global flags + command flags
        var constructor = command.Command.GetConstructor(stackTypes);
        if (constructor != null) {
            var instance = constructor.Invoke(stack.ToArray());
            return instance is T tInstance ? tInstance : default;
        }

        // all types + command flags
        constructor = command.Command.GetConstructor(stackTypes.Skip(1).ToArray());
        if (constructor != null) {
            var instance = constructor.Invoke(stack.Skip(1).ToArray());
            return instance is T tInstance ? tInstance : default;
        }

        // global flags + command flags
        constructor = command.Command.GetConstructor(stackTypes.Take(2).ToArray());
        if (constructor != null) {
            var instance = constructor.Invoke(stack.Take(2).ToArray());
            return instance is T tInstance ? tInstance : default;
        }

        // command flags
        constructor = command.Command.GetConstructor(stackTypes.Take(1).ToArray());
        if (constructor != null) {
            var instance = constructor.Invoke(stack.Take(1).ToArray());
            return instance is T tInstance ? tInstance : default;
        }

        Console.WriteLine($"Command {commandName} is not configured properly. Lacking a valid constructor");
        return default;
    }
}
