// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DragonLib.CommandLine;

public delegate void PrintHelpDelegate(Dictionary<PropertyInfo, (FlagAttribute Flag, Type FlagType)> flags, object instance, CommandLineOptions options, bool helpInvoked);

public delegate void PrintVersionDelegate(object instance, CommandLineOptions options);

public static class CommandLineFlagsParser {
	public static void PrintHelp<T>(CommandLineOptions options, bool helpInvoked) => PrintHelp(typeof(T), options, helpInvoked);

	public static void PrintHelp(Type t, CommandLineOptions options, bool helpInvoked) => options.HelpDelegate(GetFlags(t), Activator.CreateInstance(t)!, options, helpInvoked);

	public static void PrintHelpInvoker<T>(Dictionary<PropertyInfo, (FlagAttribute Flag, Type FlagType)> flags, object instance, CommandLineOptions options, bool helpInvoked) => PrintHelp(GetFlags(typeof(T)), instance, options, helpInvoked);

	public static Dictionary<PropertyInfo, (FlagAttribute Flag, Type PropertyType)> GetFlags(Type t) {
		var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
		var typeMap = properties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).ToDictionary(x => x.x, y => (y.Item2, y.x.PropertyType));
		var propertyNameToProperty = properties.ToDictionary(x => x.Name, y => y);
		foreach (var @interface in t.GetInterfaces()) {
			var interfaceProperties = @interface.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
			foreach (var (prop, info) in interfaceProperties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).Where(x => x.Item2 is not null)) {
				if (!propertyNameToProperty.TryGetValue(prop.Name, out var propertyImplementation) ||
					!typeMap.TryGetValue(propertyImplementation, out var propertySet) || propertySet.Item1 is null) {
					continue;
				}

				propertySet.Item1 = info;
				typeMap[propertyImplementation] = propertySet;
			}
		}

		typeMap = typeMap.Where(x => x.Value.Item1 is not null).ToDictionary(x => x.Key, y => y.Value);
		return typeMap!;
	}

	public static void PrintHelp(Dictionary<PropertyInfo, (FlagAttribute Flag, Type FlagType)> flags, object instance, CommandLineOptions options, bool helpInvoked) {
		flags = flags.Where(x => x.Value.Flag.Hidden == false).ToDictionary(x => x.Key, y => y.Value);
		var entry = Assembly.GetEntryAssembly()?.GetName();
		var usageSlim = "Usage: ";
		if (entry is not null) {
			usageSlim += $"{entry.Name} ";
		}

		if (!string.IsNullOrEmpty(options.Command)) {
			usageSlim += $"{options.Command} ";
		}

		var sizes = new[] { 0, 0, 0, 0 };
		var usageSlimOneCh = "-";
		var usageSlimOneChValue = string.Empty;
		var usageSlimOneChValueOptional = string.Empty;
		var usageSlimOneChOptional = "[-";
		var usageSlimMultiCh = string.Empty;
		var usageSlimPositional = string.Empty;
		foreach (var (_, (flag, originalType)) in flags) {
			var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
			var tn = type.Name;

			if (type.FullName is "System.IO.DirectoryInfo" or "System.IO.FileInfo") {
				tn = "path";
			}

			if (type.IsConstructedGenericType) {
				var parameters = type.GetGenericArguments().Select(x => x.Name);
				tn = type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)] + $"<{string.Join(", ", parameters)}>";
			}

			sizes[1] = Math.Max(sizes[1], tn.Length);
			var hasValue = type.FullName != "System.Boolean";
			var flagStr = flag.Flag;
			if (flag.Positional == -1) {
				flagStr = string.Join(", ", flag.Flags.Select(sw => $"-{(sw.Length > 1 ? "-" : string.Empty)}{sw}{(hasValue ? " value" : string.Empty)}"));
			}

			sizes[0] = Math.Max(sizes[0], flagStr.Length);

			if (!string.IsNullOrEmpty(flag.Env)) {
				sizes[2] = Math.Max(sizes[2], flag.Env.Length + 1);
			}

			if (flag.Positional == -1) {
				var flagOne = flag.Flags.FirstOrDefault(x => x.Length == 1);
				var flagTwo = flag.Flags.FirstOrDefault(x => x.Length > 1);
				if (type.FullName == "System.Boolean") {
					if (flag.IsRequired) {
						if (flagOne is not null) {
							usageSlimOneCh += flagOne;
						}

						if (flagTwo is not null) {
							usageSlimMultiCh += $"--{flagTwo} ";
						}
					} else {
						if (flagOne is not null) {
							usageSlimOneChOptional += flagOne;
						}

						if (flagTwo is not null) {
							usageSlimMultiCh += $"[--{flagTwo}] ";
						}
					}
				} else {
					if (flag.IsRequired) {
						if (flagOne is not null) {
							usageSlimOneChValue += $"-{flagOne} value ";
						}

						if (flagTwo is not null) {
							usageSlimMultiCh += $"--{flagTwo} value ";
						}
					} else {
						if (flagOne is not null) {
							usageSlimOneChValueOptional += $"[-{flagOne} value] ";
						}

						if (flagTwo is not null) {
							usageSlimMultiCh += $"[--{flagTwo} value] ";
						}
					}
				}
			} else {
				var positional = flag.Flag;
				if (type.IsEquivalentTo(typeof(List<string>)) || type.IsEquivalentTo(typeof(HashSet<string>))) {
					positional += "...";
				}

				if (flag.IsRequired) {
					usageSlimPositional += $"<{positional}> ";
				} else {
					usageSlimPositional += $"[{positional}] ";
				}
			}
		}

		if (usageSlimOneCh.Length > 1) {
			usageSlim += usageSlimOneCh + " ";
		}

		if (usageSlimOneChOptional.Length > 2) {
			usageSlim += usageSlimOneChOptional + "] ";
		}

		if (usageSlimOneChValue.Length > 1) {
			usageSlim += usageSlimOneChValue;
		}

		if (usageSlimOneChValueOptional.Length > 1) {
			usageSlim += usageSlimOneChValueOptional;
		}

		if (usageSlimMultiCh.Length > 1) {
			usageSlim += usageSlimMultiCh;
		}

		if (usageSlimPositional.Length > 1) {
			usageSlim += usageSlimPositional;
		}

		Console.WriteLine(usageSlim.TrimEnd());
		Console.WriteLine(string.Empty);
		Console.WriteLine("{0} {1} {2}{3}",
			"switch".PadRight(sizes[0]),
			"type".PadRight(sizes[1]),
			sizes[2] > 0 ? "env".PadRight(sizes[2]) : string.Empty,
			"help");

		foreach (var positionalGroup in flags.GroupBy(x => x.Value.Flag.Positional > -1).OrderByDescending(x => x.Key)) {
			foreach (var (property, (flag, originalType)) in positionalGroup) {
				var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
				var hasValue = type.FullName != "System.Boolean";
				var tn = type.Name;
				if (type.IsConstructedGenericType) {
					var parameters = type.GetGenericArguments().Select(x => x.Name);
					tn = type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)] + $"<{string.Join(", ", parameters)}>";
				}

				tn = tn.PadRight(sizes[1]);

				var requiredParts = new List<string>();
				object? def = default;
				if (type.IsValueType) {
					def = Activator.CreateInstance(type);
				}

				var defaultValue = GetDefaultValue(property, instance);
				if (defaultValue is not null && !defaultValue.Equals(def) && (type.IsValueType || type.FullName == "System.String")) {
					requiredParts.Add($"Default: {(type.IsEnum ? ((Enum) defaultValue).ToString("F") : defaultValue.ToString())}");
				}

				if (flag.IsRequired) {
					requiredParts.Add("Required");
				}

				if (flag.ValidValues?.Length > 0) {
					requiredParts.Add("Values: " + string.Join(", ", flag.ValidValues));
				} else if (type.IsEnum) {
					var names = Enum.GetNames(type);
					if (flag.EnumPrefix?.Length > 0) {
						names = names.Select(x => {
										 var prefix = flag.EnumPrefix.FirstOrDefault(y => x.StartsWith(y, StringComparison.OrdinalIgnoreCase));
										 return prefix is not null ? x[prefix.Length..] : x;
									 })
									 .ToArray();
					}

					requiredParts.Add("Values: " + string.Join(", ", helpInvoked ? names : names.Take(3)));
					if (!helpInvoked && names.Length > 3) {
						requiredParts[^1] += $", and {names.Length - 3} more";
					}
				}

				var te = string.Empty;
				if (sizes[2] > 0) {
					if (!string.IsNullOrEmpty(flag.Env)) {
						te += flag.Env;
						if (flag.EnvSeparator != 0) {
							requiredParts.Add($"Env Separator: '{flag.EnvSeparator}'");
						}
					}

					te = te.PadRight(sizes[2]);
				}

				var required = string.Join(", ", requiredParts);
				if (required.Length > 0) {
					required = $"({required})";
				}

				var flagStr = flag.Flag;
				if (flag.Positional == -1) {
					flagStr = string.Join(", ", flag.Flags.Select(sw => $"-{(sw.Length > 1 ? "-" : string.Empty)}{sw}{(hasValue ? " value" : string.Empty)}"));
				}

				Console.WriteLine("{0} {1} {2}{3} {4}", flagStr.PadRight(sizes[0]), tn, te, flag.Help?.Trim() ?? string.Empty, required.Trim());
			}
		}
	}

	public static void PrintVersion(object instance, CommandLineOptions options) => Console.WriteLine($"{AppDomain.CurrentDomain.FriendlyName} version {Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0"}");

	public static CommandLineFlags ParseFlags(Type t) => (CommandLineFlags) typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [])?.MakeGenericMethod(t).Invoke(default, [])!;

	public static CommandLineFlags ParseFlags(Type t, CommandLineOptions options) => (CommandLineFlags) typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(CommandLineOptions)])?.MakeGenericMethod(t).Invoke(default, [options])!;

	public static CommandLineFlags ParseFlags(Type t, params string[] arguments) => (CommandLineFlags) typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(string[])])?.MakeGenericMethod(t).Invoke(default, [arguments])!;

	public static CommandLineFlags ParseFlags(Type t, CommandLineOptions options, params string[] arguments) => (CommandLineFlags) typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(CommandLineOptions), typeof(string[])])?.MakeGenericMethod(t).Invoke(default, [options, arguments])!;

	public static T ParseFlags<T>() where T : CommandLineFlags => ParseFlags<T>(Environment.GetCommandLineArgs().Skip(1).ToArray());

	public static T ParseFlags<T>(CommandLineOptions options) where T : CommandLineFlags => ParseFlags<T>(options, Environment.GetCommandLineArgs().Skip(1).ToArray());

	public static T ParseFlags<T>(params string[] arguments) where T : CommandLineFlags => ParseFlags<T>(CommandLineOptions.Default, arguments);

	public static T ParseFlags<T>(CommandLineOptions options, params string[] arguments) where T : CommandLineFlags {
		var shouldExit = false;
		var instance = Activator.CreateInstance<T>();
		var typeMap = GetFlags(typeof(T));
		var propertyNameToProperty = typeMap.Keys.ToDictionary(x => x.Name, y => y);
		var argMap = new Dictionary<string, HashSet<int>>();
		var positionalMap = new HashSet<int>();
		var skipped = options.SkipPositionals;
		for (var index = 0; index < arguments.Length; index++) {
			var argument = arguments[index];
			if (argument.StartsWith('-')) {
				if (argument.StartsWith("--")) {
					if (!argMap.TryGetValue(argument[2..], out var argIndex)) {
						argIndex = [];
						argMap[argument[2..]] = argIndex;
					}

					argIndex.Add(index);
				} else {
					foreach (var argc in argument[1..]) {
						if (!argMap.TryGetValue(argc.ToString(), out var argIndex)) {
							argIndex = [];
							argMap[argc.ToString()] = argIndex;
						}

						argIndex.Add(index);
					}
				}
			} else {
				if (skipped-- > 0) {
					continue;
				}

				positionalMap.Add(index);
			}
		}

		if (options.UseHelp && propertyNameToProperty.TryGetValue("Help", out var helpProperty) && typeMap.TryGetValue(helpProperty, out var helpEntry)) {
			if (helpEntry.Flag.Flags.Any(flag => argMap.ContainsKey(flag))) {
				options.HelpDelegate(typeMap, instance, options, true);
				goto exit;
			}
		}

		if (options.UseVersion && propertyNameToProperty.TryGetValue("Version", out var versionProperty) && typeMap.TryGetValue(versionProperty, out var versionEntry)) {
			if (versionEntry.Flag.Flags.Any(flag => argMap.ContainsKey(flag))) {
				options.VersionDelegate(instance, options);
				goto exit;
			}
		}

		foreach (var (property, (flag, originalType)) in typeMap.Where(x => !string.IsNullOrEmpty(x.Value.Flag.Env))) {
			var textValueRaw = Environment.GetEnvironmentVariable(flag.Env!);
			if (string.IsNullOrEmpty(textValueRaw)) {
				continue;
			}

			var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
			var isNullable = type != originalType;
			var value = GetDefaultValue(property, instance);
			var shouldSet = true;
			foreach (var textValue in ParseTextValues(flag, textValueRaw)) {
				if (type.IsConstructedGenericType && (type.GetGenericTypeDefinition().IsEquivalentTo(typeof(List<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(Collection<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(HashSet<>)))) {
					var temp = default(object?);
					value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
					var values = new List<string>();
					if (flag.EnvSeparator != 0) {
						values.AddRange(SplitEscaping(textValue, flag.EnvSeparator).Select(x => x.Trim()).Where(x => x.Length > 0));
					} else {
						values.Add(textValue);
					}

					foreach (var splitValue in values) {
						if (VisitFlagValue<T>(type.GetGenericArguments()[0], splitValue, flag, ref temp)) {
							goto fail;
						}

						type.GetMethod("Add")?.Invoke(value, [temp]);
					}

					shouldSet = false;
				} else if (VisitFlagValue<T>(type, textValue, flag, ref value)) {
					goto fail;
				}
			}

			if (!shouldSet) {
				continue;
			}

			if (isNullable) {
				value = value is null ? Activator.CreateInstance(originalType) : Activator.CreateInstance(originalType, value);
			} else if (type != typeof(string)) {
				try {
					value ??= Activator.CreateInstance(type);
				} catch {
					// ignored
				}
			}

			property.SetValue(instance, value);
		}

		foreach (var (property, (flag, originalType)) in typeMap.Where(x => x.Value.Flag.Positional == -1)) {
			var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
			var isNullable = type != originalType;

			var indexList = default(HashSet<int>);
			foreach (var sw in flag.Flags) {
				if (argMap.TryGetValue(sw, out indexList)) {
					break;
				}
			}

			if (indexList is null && flag.IsRequired) {
				Console.WriteLine($"{flag.Flag} needs a value");
				goto fail;
			}

			var value = GetDefaultValue(property, instance);
			var shouldSet = true;
			if (indexList is not null) {
				foreach (var index in indexList) {
					if (type.FullName == "System.Boolean") {
						if (value is not bool b) {
							b = false;
						}

						value = !b;
					} else {
						var argument = arguments[index];
						var hasInnateValue = argument.Contains('=', StringComparison.Ordinal);
						string textValueRaw;
						if (hasInnateValue) {
							textValueRaw = argument[(argument.IndexOf('=', StringComparison.Ordinal) + 1)..];
							if (string.IsNullOrWhiteSpace(textValueRaw)) {
								Console.WriteLine($"{flag.Flag} needs a value", flag.Flag);
								goto fail;
							}
						} else {
							if (!positionalMap.Contains(index + 1)) {
								Console.WriteLine($"{flag.Flag} needs a value", flag.Flag);
								goto fail;
							}

							textValueRaw = arguments[index + 1];
							positionalMap.Remove(index + 1);
						}

						foreach (var textValue in ParseTextValues(flag, textValueRaw)) {
							switch (type.IsConstructedGenericType) {
								case true when type.GetGenericTypeDefinition().IsEquivalentTo(typeof(List<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(Collection<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(HashSet<>)): {
									var listValue = default(object?);
									if (VisitFlagValue<T>(type.GetGenericArguments()[0], textValue, flag, ref listValue)) {
										goto fail;
									}

									value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
									type.GetMethod("Add")?.Invoke(value, [listValue]);
									shouldSet = false;
									break;
								}
								case true when type.GetGenericTypeDefinition().IsEquivalentTo(typeof(Dictionary<,>)): {
									var parts = textValue.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.None);

									var keyValue = default(object?);
									if (VisitFlagValue<T>(type.GetGenericArguments()[0], parts[0], flag, ref keyValue)) {
										goto fail;
									}

									var valueValue = default(object?);
									if (VisitFlagValue<T>(type.GetGenericArguments()[1], parts[1], flag, ref valueValue)) {
										goto fail;
									}

									value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
									type.GetMethod("Add")?.Invoke(value, [keyValue, valueValue]);
									shouldSet = false;
									break;
								}
								default: {
									if (type.IsAssignableTo(typeof(IList))) {
										var listValue = default(object?);
										if (VisitFlagValue<T>(typeof(string), textValue, flag, ref listValue)) {
											goto fail;
										}

										value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
										type.GetMethod("Add")?.Invoke(value, [listValue]);
										shouldSet = false;
									} else if (type.IsAssignableTo(typeof(IDictionary))) {
										var parts = textValue.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.None);

										var keyValue = default(object?);
										if (VisitFlagValue<T>(typeof(string), parts[0], flag, ref keyValue)) {
											goto fail;
										}

										var valueValue = default(object?);
										if (VisitFlagValue<T>(typeof(string), parts[1], flag, ref valueValue)) {
											goto fail;
										}

										value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
										type.GetMethod("Add")?.Invoke(value, [keyValue, valueValue]);
										shouldSet = false;
									} else if (VisitFlagValue<T>(type, textValue, flag, ref value)) {
										goto fail;
									}

									break;
								}
							}
						}
					}
				}
			}

			if (!shouldSet) {
				continue;
			}

			if (isNullable) {
				value = value is null ? Activator.CreateInstance(originalType) : Activator.CreateInstance(originalType, value);
			} else if (type != typeof(string)) {
				try {
					value ??= Activator.CreateInstance(type);
				} catch {
					// ignored
				}
			}

			property.SetValue(instance, value);
		}

		var positionals = positionalMap.Select(x => arguments[x]).ToList();
		foreach (var (property, (flag, originalType)) in typeMap.Where(x => x.Value.Flag.Positional > -1)) {
			var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
			var isNullable = type != originalType;

			if (flag.IsRequired && flag.Positional >= positionalMap.Count) {
				Console.WriteLine($"Positional {flag.Flag} needs a value");
				goto fail;
			}

			var value = GetDefaultValue(property, instance);
			var shouldSet = true;
			if (type.IsConstructedGenericType && (type.GetGenericTypeDefinition().IsEquivalentTo(typeof(List<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(Collection<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(HashSet<>)))) {
				var temp = default(object?);
				value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
				foreach (var textValueRaw in positionals.Skip(flag.Positional)) {
					foreach (var textValue in ParseTextValues(flag, textValueRaw)) {
						if (VisitFlagValue<T>(type.GetGenericArguments()[0], textValue, flag, ref temp)) {
							goto fail;
						}

						type.GetMethod("Add")?.Invoke(value, [temp]);
					}
				}

				shouldSet = false;
			} else if (positionals.Count > flag.Positional) {
				foreach (var textValue in ParseTextValues(flag, positionals[flag.Positional])) {
					if (!VisitFlagValue<T>(type, textValue, flag, ref value)) {
						continue;
					}

					goto fail;
				}
			}

			if (!shouldSet) {
				continue;
			}

			if (isNullable) {
				value = value is null ? Activator.CreateInstance(originalType) : Activator.CreateInstance(originalType, value);
			} else if (type != typeof(string)) {
				try {
					value ??= Activator.CreateInstance(type);
				} catch {
					// ignored
				}
			}

			property.SetValue(instance, value);
		}

	end:
		if (options.UseHelp && instance.Help) {
			options.HelpDelegate(typeMap, instance, options, instance.Help);
			goto exit;
		}

		if (options.UseVersion && instance.Version) {
			options.VersionDelegate(instance, options);
			goto exit;
		}

		if (!shouldExit) {
			return instance;
		}

	exit:
		Environment.Exit(0);
		throw new UnreachableException();

	fail:
		shouldExit = true;
		goto end;
	}

	private static IEnumerable<string> ParseTextValues(FlagAttribute flag, string textValueRaw) {
		if (flag.FileListPrefix != 0 && textValueRaw.StartsWith(flag.FileListPrefix) && File.Exists(textValueRaw[1..])) {
			foreach (var line in File.ReadAllLines(textValueRaw[1..])) {
				yield return line;
			}
		} else {
			yield return textValueRaw;
		}
	}

	private static IEnumerable<string> SplitEscaping(string str, char split) {
		var sb = new StringBuilder(str.Length);

		for (var i = 0; i < str.Length; ++i) {
			var ch = str[i];

			if (ch == split) {
				yield return sb.ToString();
				sb.Clear();
			}

			sb.Append(ch == '\\' ? str[++i] : ch);
		}

		yield return sb.ToString();
	}

	private static object? GetDefaultValue(PropertyInfo property, object instance) {
		var value = property.GetValue(instance);

		if (value is null) {
			return default;
		}

		var originalType = value.GetType();
		var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
		var isNullable = type != originalType;
		if (isNullable) {
			return type.GetProperty("HasValue")?.GetValue(value) as bool? != true ? default : type.GetProperty("Value")?.GetValue(value);
		}

		return value;
	}

	private static bool VisitFlagValue<T>(Type type, string textValue, FlagAttribute flag, ref object? value) where T : CommandLineFlags {
		var sterilizedValue = textValue;
		if (flag.ReplaceDashes > 0) {
			sterilizedValue = sterilizedValue.Replace('-', flag.ReplaceDashes);
		}

		if (flag.ReplaceDots > 0) {
			sterilizedValue = sterilizedValue.Replace('.', flag.ReplaceDashes);
		}

		if (flag.ValidValues?.Length > 0 && !flag.ValidValues.Contains(sterilizedValue)) {
			Console.WriteLine($"Unrecognized value {sterilizedValue} for {flag.Flag}! Valid values are {string.Join(", ", flag.ValidValues)}");
			return true;
		}

		if (type.IsEnum) {
			if (sterilizedValue.Contains('|', StringComparison.Ordinal)) {
				var enumValue = 0UL;
				foreach (var sterilizedValuePart in sterilizedValue.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)) {
					if (!ParseEnumValue(type, flag, out var temp, sterilizedValuePart)) {
						continue;
					}

					enumValue |= Convert.ToUInt64(temp);
				}

				value = Enum.ToObject(type, enumValue);
			} else {
				if (ParseEnumValue(type, flag, out value, sterilizedValue)) {
					return false;
				}
			}

			var style = flag.GetExtraOrDefault(NumberStyles.AllowHexSpecifier | NumberStyles.Number);

			if (ulong.TryParse(sterilizedValue, style, CultureInfo.InvariantCulture, out var tempValue)) {
				value = Enum.ToObject(type, tempValue);
				return true;
			}

			if (long.TryParse(sterilizedValue, style, CultureInfo.InvariantCulture, out var tempValueSigned)) {
				value = Enum.ToObject(type, tempValueSigned);
				return true;
			}

			Console.WriteLine($"Unrecognized value {textValue} for {flag.Flag}! Valid values are {string.Join(", ", Enum.GetNames(type))}");
		} else {
			try {
				value = type.FullName switch {
					"System.String" => sterilizedValue,
					"System.Uri" => new Uri(textValue, flag.GetExtraOrDefault(UriKind.Absolute)),
					"System.Int128" => Int128.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Integer)),
					"System.UInt128" => UInt128.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.HexNumber)),
					"System.Int64" => long.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Integer)),
					"System.UInt64" => ulong.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.HexNumber)),
					"System.IntPtr" => nint.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.HexNumber)),
					"System.UIntPtr" => nuint.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.HexNumber)),
					"System.Int32" => int.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Integer)),
					"System.UInt32" => uint.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.HexNumber)),
					"System.Int16" => short.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Integer)),
					"System.UInt16" => ushort.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.HexNumber)),
					"System.SByte" => sbyte.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Integer)),
					"System.Byte" => byte.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.HexNumber)),
					"System.Runtime.InteropServices.CLong" => new CLong(nint.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Integer))),
					"System.Runtime.InteropServices.CULong" => new CULong(nuint.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.HexNumber))),
					"System.Runtime.InteropServices.NFloat" => NFloat.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Float | NumberStyles.AllowThousands)),
					"System.Numerics.BigInteger" => BigInteger.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Float | NumberStyles.AllowThousands)),
					"System.Double" => double.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Float | NumberStyles.AllowThousands)),
					"System.Single" => float.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Float | NumberStyles.AllowThousands)),
					"System.Half" => Half.Parse(textValue, flag.GetExtraOrDefault(NumberStyles.Float | NumberStyles.AllowThousands)),
					"System.Text.RegularExpressions.Regex" => new Regex(textValue, flag.GetExtraOrDefault(RegexOptions.Compiled)),
					"System.Boolean" => textValue.Length > 0 && char.ToLowerInvariant(textValue[0]) is 't' or '1' or 'y',
					"System.TimeSpan" => TimeSpan.Parse(textValue),
					"System.DateTime" => DateTime.Parse(textValue),
					"System.DateTimeOffset" => DateTimeOffset.Parse(textValue),
					"System.Guid" => Guid.Parse(textValue),
					"System.Version" => Version.Parse(textValue),
					"System.IO.DirectoryInfo" => new DirectoryInfo(textValue),
					"System.IO.FileInfo" => new FileInfo(textValue),
					_ => InvokeVisitor<T>(flag, type, textValue),
				};
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
				Console.WriteLine($"{flag.Flag} failed to parse {textValue} as a {type.Name}");
				return true;
			}
		}

		return false;
	}

	private static bool ParseEnumValue(Type type, FlagAttribute flag, out object? value, string sterilizedValue) {
		if (Enum.TryParse(type, sterilizedValue, true, out value)) {
			return true;
		}

		if (flag.EnumPrefix is {
			Length: > 0,
		}) {
			foreach (var prefix in flag.EnumPrefix) {
				if (Enum.TryParse(type, prefix + sterilizedValue, false, out value)) {
					return true;
				}
			}
		}

		return false;
	}

	private static object? InvokeVisitor<T>(FlagAttribute flag, Type type, string textValue) where T : CommandLineFlags {
		if (flag.Visitor is null) {
			throw new InvalidCastException($"Cannot process {type.FullName}");
		}

		var visitorClassName = flag.Visitor[..flag.Visitor.LastIndexOf('.')];
		var visitorMethodName = flag.Visitor[flag.Visitor.LastIndexOf('.')..];
		var visitorAssembly = flag.VisitorAssembly ?? typeof(T).Assembly;
		var visitorClass = visitorAssembly.GetType(visitorClassName);
		if (visitorClass is null) {
			throw new InvalidDataException($"Cannot find visitor class {visitorClassName}");
		}

		var visitorMethod = visitorClass.GetMethod(visitorMethodName, BindingFlags.Static);
		if (visitorMethod is null) {
			throw new InvalidDataException($"Cannot find visitor method {visitorMethodName}");
		}

		var parameters = visitorMethod.GetParameters();
		if (visitorMethod.ReturnType.FullName != "System.Void" || parameters.Length != 1 || parameters[0].ParameterType.FullName != "System.String") {
			throw new InvalidDataException($"Visitor method {visitorClassName}.{visitorMethodName} does not match delegate template Func<in string, out object>");
		}

		return visitorMethod.Invoke(default, [textValue]);
	}

	public static string ReconstructArgs(CommandLineFlags inst) {
		var type = inst.GetType();

		var sb = new StringBuilder();

		var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
		var typeMap = properties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).ToDictionary(x => x.x, y => y.Item2);
		var flags = typeMap.Where(x => x.Value?.Hidden == false).ToDictionary(x => x.Key, y => y.Value);

		foreach (var (prop, flag) in flags) {
			if (flag is null || flag.Positional > -1) {
				continue;
			}

			var value = prop.GetValue(inst);
			if (value is null or false) {
				continue;
			}

			sb.Append("--");
			sb.Append(flag.Flag);
			sb.Append(' ');

			var strValue = !prop.PropertyType.IsEnum ? value.ToString()! : ((Enum) value).ToString(prop.PropertyType.GetCustomAttribute<FlagsAttribute>() is not null ? "F" : "G");

			if (strValue.Contains(' ', StringComparison.Ordinal)) {
				sb.Append('"');
				sb.Append(strValue);
				sb.Append('"');
			} else {
				sb.Append(strValue);
			}

			sb.Append(' ');
		}

		sb.Append(string.Join(' ', inst.Positionals.Select(x => x.Contains(' ', StringComparison.Ordinal) ? $"\"{x}\"" : x)));

		return sb.ToString();
	}
}
