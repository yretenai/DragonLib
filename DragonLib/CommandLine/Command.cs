// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Reflection;

namespace DragonLib.CommandLine;

public static class Command {
	//                        group,             name
	public static Dictionary<string, Dictionary<string, (string Description, Type Type, Type Command, bool Hide)>> Commands { get; } = new() { { string.Empty, new Dictionary<string, (string Description, Type Type, Type Command, bool Hide)>() } };

	private static void LoadCommands() {
		var types = Assembly.GetEntryAssembly()?.GetTypes() ?? Type.EmptyTypes;
		foreach (var type in types) {
			var commandAttributes = type.GetCustomAttributes().Where(x => x is ICommandAttribute);
			foreach (var attribute in commandAttributes) {
				var commandAttribute = (ICommandAttribute) attribute;
				var flagsType = commandAttribute.FlagsType;
				if (!Commands.TryGetValue(commandAttribute.Group, out var value)) {
					value = new Dictionary<string, (string Description, Type Type, Type Command, bool)>();
					Commands[commandAttribute.Group] = value;
				}

				value[commandAttribute.Name] = (commandAttribute.Description, flagsType, type, commandAttribute.Hide);
			}
		}
	}

	public static void Run(out string? commandName, out string? commandGroupName, CommandLineFlags? globalFlags = default, CommandLineOptions? options = default, object[]? carry = default, string[]? args = default, char suffixSeparator = '\0') => Run<object>(out commandName, out commandGroupName, globalFlags, options, carry, args);

	public static T? Run<T>(out string? commandName, out string? commandGroupName, CommandLineFlags? globalFlags = default, CommandLineOptions? options = default, object[]? carry = default, string[]? args = default, char suffixSeparator = '\0') {
		LoadCommands();

		commandName = default;
		commandGroupName = default;

		if (args is null) {
			var envArgs = Environment.GetCommandLineArgs();
			var procName = Path.GetFileNameWithoutExtension(envArgs[0]);
			if (suffixSeparator != '\0' && procName.Contains(suffixSeparator, StringComparison.Ordinal)) {
				var procNameParts = procName.Split(suffixSeparator)[1..];
				args = procNameParts.Concat(envArgs[1..]).ToArray();
			} else {
				args = envArgs[1..];
			}
		}

		options ??= CommandLineOptions.Default;
		var positionalFlags = CommandLineFlagsParser.ParseFlags<CommandLineFlags>(new CommandLineOptions { UseHelp = false, UseVersion = false });

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

		var flags = CommandLineFlagsParser.ParseFlags(command.Type, options with { Command = $"{commandGroupName} {commandName}".Trim(), SkipPositionals = offset }, args);

		var stack = new object[2 + (carry?.Length ?? 0)];
		stack[0] = globalFlags;
		stack[1] = flags;
		if (carry is not null) {
			Array.Copy(carry, 0, stack, 2, carry.Length);
		}

		var stackTypes = stack.Select(x => x.GetType()).ToArray();

		// all types + global flags + command flags
		var constructor = command.Command.GetConstructor(stackTypes);
		if (constructor is not null) {
			var instance = constructor.Invoke(stack.ToArray());
			return instance is T tInstance ? tInstance : default;
		}

		// all types + command flags
		constructor = command.Command.GetConstructor(stackTypes.Skip(1).ToArray());
		if (constructor is not null) {
			var instance = constructor.Invoke(stack.Skip(1).ToArray());
			return instance is T tInstance ? tInstance : default;
		}

		// global flags + command flags
		constructor = command.Command.GetConstructor(stackTypes.Take(2).ToArray());
		if (constructor is not null) {
			var instance = constructor.Invoke(stack.Take(2).ToArray());
			return instance is T tInstance ? tInstance : default;
		}

		// command flags
		constructor = command.Command.GetConstructor(stackTypes.Take(1).ToArray());
		if (constructor is not null) {
			var instance = constructor.Invoke(stack.Take(1).ToArray());
			return instance is T tInstance ? tInstance : default;
		}

		Console.WriteLine($"Command {commandName} is not configured properly. Lacking a valid constructor");
		return default;
	}
}
