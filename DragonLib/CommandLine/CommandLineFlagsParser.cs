using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DragonLib.CommandLine;

public static class CommandLineFlagsParser {
	public delegate void PrintHelpDelegate(Dictionary<PropertyInfo, (FlagAttribute Flag, Type FlagType)> flags, object instance, CommandLineOptions options, bool helpInvoked);

	public static void PrintHelp<T>(PrintHelpDelegate printHelp, CommandLineOptions options, bool helpInvoked) {
		PrintHelp(typeof(T), printHelp, options, helpInvoked);
	}

	public static void PrintHelp(Type t, PrintHelpDelegate printHelp, CommandLineOptions options, bool helpInvoked) {
		printHelp.Invoke(GetFlags(t), Activator.CreateInstance(t)!, options, helpInvoked);
	}

	public static Dictionary<PropertyInfo, (FlagAttribute Flag, Type PropertyType)> GetFlags(Type t) {
		var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
		var typeMap = properties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).ToDictionary(x => x.x, y => (y.Item2, y.x.PropertyType));
		var propertyNameToProperty = properties.ToDictionary(x => x.Name, y => y);
		foreach (var @interface in t.GetInterfaces()) {
			var interfaceProperties = @interface.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
			foreach (var (prop, info) in interfaceProperties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).Where(x => x.Item2 != null)) {
				if (!propertyNameToProperty.TryGetValue(prop.Name, out var propertyImplementation) ||
				    !typeMap.TryGetValue(propertyImplementation, out var propertySet) || propertySet.Item1 == null) {
					continue;
				}

				propertySet.Item1 = info;
				typeMap[propertyImplementation] = propertySet;
			}
		}

		typeMap = typeMap.Where(x => x.Value.Item1 != null).ToDictionary(x => x.Key, y => y.Value);
		return typeMap!;
	}

	public static void PrintHelp(Dictionary<PropertyInfo, (FlagAttribute Flag, Type FlagType)> flags, object instance, CommandLineOptions options, bool helpInvoked) {
		flags = flags.Where(x => x.Value.Flag.Hidden == false).ToDictionary(x => x.Key, y => y.Value);
		var grouped = flags.GroupBy(x => x.Value.Flag.Category ?? string.Empty).Select(x => (x.Key, x.ToArray())).ToArray();
		var entry = Assembly.GetEntryAssembly()?.GetName();
		var usageSlim = "Usage: ";
		if (entry != null) {
			usageSlim += $"{entry.Name} ";
		}

		if (!string.IsNullOrEmpty(options.Command)) {
			usageSlim += $"{options.Command} ";
		}

		var sizes = new[] { 0, 0, 0 };
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

			if (flag.Positional > -1) {
				sizes[2] = Math.Max(sizes[2], $"Positional {flag.Positional + 1 + options.SkipPositionals}".Length);
			}

			sizes[1] = Math.Max(sizes[1], tn.Length);
			var hasValue = type.FullName != "System.Boolean";
			var flagStr = flag.Flag;
			if (flag.Positional == -1) {
				flagStr = string.Join(", ", flag.Flags.Select(sw => $"-{(sw.Length > 1 ? "-" : string.Empty)}{sw}{(hasValue ? " value" : string.Empty)}"));
			}

			sizes[0] = Math.Max(sizes[0], flagStr.Length);
			if (flag.Positional == -1) {
				var flagOne = flag.Flags.FirstOrDefault(x => x.Length == 1);
				var flagTwo = flag.Flags.FirstOrDefault(x => x.Length > 1);
				if (type.FullName == "System.Boolean") {
					if (flag.IsRequired) {
						if (flagOne != null) {
							usageSlimOneCh += flagOne;
						}

						if (flagTwo != null) {
							usageSlimMultiCh += $"--{flagTwo} ";
						}
					} else {
						if (flagOne != null) {
							usageSlimOneChOptional += flagOne;
						}

						if (flagTwo != null) {
							usageSlimMultiCh += $"[--{flagTwo}] ";
						}
					}
				} else {
					if (flag.IsRequired) {
						if (flagOne != null) {
							usageSlimOneChValue += $"-{flagOne} value ";
						}

						if (flagTwo != null) {
							usageSlimMultiCh += $"--{flagTwo} value ";
						}
					} else {
						if (flagOne != null) {
							usageSlimOneChValueOptional += $"[-{flagOne} value] ";
						}

						if (flagTwo != null) {
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
		foreach (var (group, attributes) in grouped) {
			if (!string.IsNullOrEmpty(group)) {
				Console.WriteLine($"{group}: ");
			}

			foreach (var (property, (flag, originalType)) in attributes) {
				var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
				var hasValue = type.FullName != "System.Boolean";
				var tn = type.Name;
				if (type.IsConstructedGenericType) {
					var parameters = type.GetGenericArguments().Select(x => x.Name);
					tn = type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)] + $"<{string.Join(", ", parameters)}>";
				}

				var tp = string.Empty;
				if (flag.Positional > -1) {
					tp += $"Positional {flag.Positional + 1 + options.SkipPositionals}";
				}

				tn = tn.PadRight(sizes[1]);
				tp = tp.PadLeft(sizes[2]);
				var requiredParts = new List<string>();
				object? def = null;
				if (type.IsValueType) {
					def = Activator.CreateInstance(type);
				}

				var defaultValue = GetDefaultValue(property, instance);
				if (defaultValue != null && !defaultValue.Equals(def)) {
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
							              return prefix != null ? x[prefix.Length..] : x;
						              })
						             .ToArray();
					}

					requiredParts.Add("Values: " + string.Join(", ", helpInvoked ? names : names.Take(3)));
					if (!helpInvoked && names.Length > 3) {
						requiredParts[^1] += $", and {names.Length - 3} more";
					}
				}

				var required = string.Join(", ", requiredParts);
				if (required.Length > 0) {
					required = $"({required})";
				}

				var flagStr = flag.Flag;
				if (flag.Positional == -1) {
					flagStr = string.Join(", ", flag.Flags.Select(sw => $"-{(sw.Length > 1 ? "-" : string.Empty)}{sw}{(hasValue ? " value" : string.Empty)}"));
				}

				Console.WriteLine("{0}\t{1}\t{2}\t{3} {4}", flagStr.PadRight(sizes[0]), tn, tp, flag.Help?.Trim(), required.Trim());
			}

			Console.WriteLine(string.Empty);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? ParseFlags<T>() where T : CommandLineFlags => ParseFlags<T>(Environment.GetCommandLineArgs().Skip(1).ToArray());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? ParseFlags<T>(CommandLineOptions options) where T : CommandLineFlags => ParseFlags<T>(options, Environment.GetCommandLineArgs().Skip(1).ToArray());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? ParseFlags<T>(params string[] arguments) where T : CommandLineFlags => ParseFlags<T>(PrintHelp, CommandLineOptions.Empty, arguments);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? ParseFlags<T>(CommandLineOptions options, params string[] arguments) where T : CommandLineFlags => ParseFlags<T>(PrintHelp, options, arguments);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandLineFlags? ParseFlags(Type t, CommandLineOptions options) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(CommandLineOptions)])?.MakeGenericMethod(t).Invoke(null, [options]) as CommandLineFlags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandLineFlags? ParseFlags(Type t, CommandLineOptions options, params string[] arguments) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(CommandLineOptions), typeof(string[])])?.MakeGenericMethod(t).Invoke(null, [options, arguments]) as CommandLineFlags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandLineFlags? ParseFlags(Type t) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [])?.MakeGenericMethod(t).Invoke(null, []) as CommandLineFlags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandLineFlags? ParseFlags(Type t, params string[] arguments) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(string[])])?.MakeGenericMethod(t).Invoke(null, [arguments]) as CommandLineFlags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandLineFlags? ParseFlags(Type t, PrintHelpDelegate printHelp, CommandLineOptions options, params string[] arguments) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(PrintHelpDelegate), typeof(CommandLineOptions), typeof(string[])])?.MakeGenericMethod(t).Invoke(null, [printHelp, options, arguments]) as CommandLineFlags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandLineFlags? ParseFlags(Type t, PrintHelpDelegate printHelp, CommandLineOptions options) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(PrintHelpDelegate), typeof(CommandLineOptions)])?.MakeGenericMethod(t).Invoke(null, [printHelp, options]) as CommandLineFlags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandLineFlags? ParseFlags(Type t, PrintHelpDelegate printHelp, params string[] arguments) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(PrintHelpDelegate), typeof(CommandLineOptions), typeof(string[])])?.MakeGenericMethod(t).Invoke(null, [printHelp, CommandLineOptions.Empty, arguments]) as CommandLineFlags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandLineFlags? ParseFlags(Type t, PrintHelpDelegate printHelp) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), [typeof(PrintHelpDelegate), typeof(CommandLineOptions)])?.MakeGenericMethod(t).Invoke(null, [printHelp, CommandLineOptions.Empty]) as CommandLineFlags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? ParseFlags<T>(PrintHelpDelegate printHelp, CommandLineOptions options) where T : CommandLineFlags => ParseFlags<T>(printHelp, options, Environment.GetCommandLineArgs().Skip(1).ToArray());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? ParseFlags<T>(PrintHelpDelegate printHelp) where T : CommandLineFlags => ParseFlags<T>(printHelp, CommandLineOptions.Empty, Environment.GetCommandLineArgs().Skip(1).ToArray());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? ParseFlags<T>(PrintHelpDelegate printHelp, params string[] arguments) where T : CommandLineFlags => ParseFlags<T>(printHelp, CommandLineOptions.Empty, arguments);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? ParseFlags<T>(PrintHelpDelegate printHelp, CommandLineOptions options, params string[] arguments) where T : CommandLineFlags {
		var shouldExit = false;
		var instance = Activator.CreateInstance<T>();
		var typeMap = GetFlags(typeof(T));
		var propertyNameToProperty = typeMap.Keys.ToDictionary(x => x.Name, y => y);
		var argMap = new Dictionary<string, HashSet<int>>();
		var positionalMap = new HashSet<int>();
		var skipped = options.SkipPositionals;
		for (var index = 0; index < arguments.Length; index++) {
			var argument = arguments[index];
			if (argument.StartsWith("-")) {
				if (argument.StartsWith("--")) {
					if (!argMap.TryGetValue(argument[2..], out var argIndex)) {
						argIndex = new HashSet<int>();
						argMap[argument[2..]] = argIndex;
					}

					argIndex.Add(index);
				} else {
					foreach (var argc in argument[1..]) {
						if (!argMap.TryGetValue(argc.ToString(), out var argIndex)) {
							argIndex = new HashSet<int>();
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
				printHelp(typeMap, instance, options, true);
				Environment.Exit(0);
				return null;
			}
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

			if (indexList == null && flag.IsRequired) {
				Console.WriteLine($"{flag.Flag} needs a value");
				shouldExit = true;
			}

			var value = GetDefaultValue(property, instance);
			var shouldSet = true;
			if (indexList != null) {
				foreach (var index in indexList) {
					if (type.FullName == "System.Boolean") {
						if (value is not bool b) {
							b = false;
						}

						value = !b;
					} else {
						var argument = arguments[index];
						var hasInnateValue = argument.Contains('=', StringComparison.Ordinal);
						string textValue;
						if (hasInnateValue) {
							textValue = argument[(argument.IndexOf('=', StringComparison.Ordinal) + 1)..];
							if (string.IsNullOrWhiteSpace(textValue)) {
								Console.WriteLine($"{flag.Flag} needs a value", flag.Flag);
								shouldExit = true;
							}
						} else {
							if (!positionalMap.Contains(index + 1)) {
								Console.WriteLine($"{flag.Flag} needs a value", flag.Flag);
								shouldExit = true;
							}

							textValue = arguments[index + 1];
							positionalMap.Remove(index + 1);
						}

						switch (type.IsConstructedGenericType) {
							case true when type.GetGenericTypeDefinition().IsEquivalentTo(typeof(List<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(Collection<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(HashSet<>)): {
								var listValue = default(object?);
								if (VisitFlagValue<T>(type.GetGenericArguments()[0], textValue, flag, ref listValue)) {
									return null;
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
									return null;
								}

								var valueValue = default(object?);
								if (VisitFlagValue<T>(type.GetGenericArguments()[1], parts[1], flag, ref valueValue)) {
									return null;
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
										return null;
									}

									value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
									type.GetMethod("Add")?.Invoke(value, [listValue]);
									shouldSet = false;
								} else if (type.IsAssignableTo(typeof(IDictionary))) {
									var parts = textValue.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.None);

									var keyValue = default(object?);
									if (VisitFlagValue<T>(typeof(string), parts[0], flag, ref keyValue)) {
										return null;
									}

									var valueValue = default(object?);
									if (VisitFlagValue<T>(typeof(string), parts[1], flag, ref valueValue)) {
										return null;
									}

									value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
									type.GetMethod("Add")?.Invoke(value, [keyValue, valueValue]);
									shouldSet = false;
								} else if (VisitFlagValue<T>(type, textValue, flag, ref value)) {
									return null;
								}

								break;
							}
						}
					}
				}
			}

			if (shouldSet) {
				if (isNullable) {
					value = value == null ? Activator.CreateInstance(originalType) : Activator.CreateInstance(originalType, value);
				} else if (type != typeof(string)) {
					try {
						value ??= Activator.CreateInstance(type);
					} catch {
						// ignored
					}
				}

				property.SetValue(instance, value);
			}
		}

		var positionals = positionalMap.Select(x => arguments[x]).ToList();
		foreach (var (property, (flag, originalType)) in typeMap.Where(x => x.Value.Flag.Positional > -1)) {
			var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
			var isNullable = type != originalType;

			if (flag.IsRequired && flag.Positional >= positionalMap.Count) {
				Console.WriteLine($"Positional {flag.Flag} needs a value");
				shouldExit = true;
			}

			var value = GetDefaultValue(property, instance);
			var shouldSet = true;
			if (type.IsConstructedGenericType && (type.GetGenericTypeDefinition().IsEquivalentTo(typeof(List<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(Collection<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(HashSet<>)))) {
				var temp = default(object?);
				value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
				foreach (var textValue in positionals.Skip(flag.Positional)) {
					if (VisitFlagValue<T>(type.GetGenericArguments()[0], textValue, flag, ref temp)) {
						return null;
					}

					type.GetMethod("Add")?.Invoke(value, [temp]);
				}

				shouldSet = false;
			} else if (positionals.Count > flag.Positional && VisitFlagValue<T>(type, positionals[flag.Positional], flag, ref value)) {
				shouldExit = true;
			}

			if (shouldSet) {
				if (isNullable) {
					value = value == null ? Activator.CreateInstance(originalType) : Activator.CreateInstance(originalType, value);
				} else if (type != typeof(string)) {
					try {
						value ??= Activator.CreateInstance(type);
					} catch {
						// ignored
					}
				}

				property.SetValue(instance, value);
			}
		}

		if (!options.UseHelp || (!instance.Help && !shouldExit)) {
			return instance;
		}

		printHelp(typeMap, instance, options, instance.Help);
		Environment.Exit(0);
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static object? GetDefaultValue(PropertyInfo property, object instance) {
		var value = property.GetValue(instance);

		if (value == null) {
			return null;
		}

		var originalType = value.GetType();
		var type = Nullable.GetUnderlyingType(originalType) ?? originalType;
		var isNullable = type != originalType;
		if (isNullable) {
			return type.GetProperty("HasValue")?.GetValue(value) as bool? != true ? null : type.GetProperty("Value")?.GetValue(value);
		}

		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

			if (ulong.TryParse(sterilizedValue, NumberStyles.AllowHexSpecifier | NumberStyles.Number, CultureInfo.InvariantCulture, out var tempValue)) {
				value = Enum.ToObject(type, tempValue);
				return true;
			}

			if (long.TryParse(sterilizedValue, NumberStyles.AllowHexSpecifier | NumberStyles.Number, CultureInfo.InvariantCulture, out var tempValueSigned)) {
				value = Enum.ToObject(type, tempValueSigned);
				return true;
			}

			Console.WriteLine($"Unrecognized value {textValue} for {flag.Flag}! Valid values are {string.Join(", ", Enum.GetNames(type))}");
		} else {
			try {
				value = type.FullName switch {
					        "System.Int64" => long.Parse(textValue, NumberStyles.Any),
					        "System.UInt64" => ulong.Parse(textValue, NumberStyles.HexNumber),
					        "System.IntPtr" => nint.Parse(textValue, NumberStyles.HexNumber),
					        "System.UIntPtr" => nuint.Parse(textValue, NumberStyles.HexNumber),
					        "System.Int32" => int.Parse(textValue, NumberStyles.Any),
					        "System.UInt32" => uint.Parse(textValue, NumberStyles.HexNumber),
					        "System.Int16" => short.Parse(textValue, NumberStyles.Any),
					        "System.UInt16" => ushort.Parse(textValue, NumberStyles.HexNumber),
					        "System.SByte" => sbyte.Parse(textValue, NumberStyles.Any),
					        "System.Byte" => byte.Parse(textValue, NumberStyles.HexNumber),
					        "System.Double" => double.Parse(textValue),
					        "System.Single" => float.Parse(textValue),
					        "System.Half" => Half.Parse(textValue),
					        "System.String" => sterilizedValue,
					        "System.Text.RegularExpressions.Regex" => new Regex(textValue, (RegexOptions) (flag.Extra ?? RegexOptions.Compiled)),
					        "DragonLib.Numerics.Half" => Half.Parse(textValue),
					        "System.TimeSpan" => TimeSpan.Parse(textValue),
					        "System.DateTime" => DateTime.Parse(textValue),
					        "System.DateTimeOffset" => DateTimeOffset.Parse(textValue),
					        "System.Guid" => Guid.Parse(textValue),
					        "System.Uri" => new Uri(textValue),
					        "System.Version" => Version.Parse(textValue),
					        "System.Numerics.BigInteger" => BigInteger.Parse(textValue),
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

		if (flag.EnumPrefix is { Length: > 0 }) {
			foreach (var prefix in flag.EnumPrefix) {
				if (Enum.TryParse(type, prefix + sterilizedValue, false, out value)) {
					return true;
				}
			}
		}

		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static object? InvokeVisitor<T>(FlagAttribute flag, Type type, string textValue) where T : CommandLineFlags {
		if (flag.Visitor == null) {
			throw new InvalidCastException($"Cannot process {type.FullName}");
		}

		var visitorClassName = flag.Visitor[..flag.Visitor.LastIndexOf(".", StringComparison.Ordinal)];
		var visitorMethodName = flag.Visitor[flag.Visitor.LastIndexOf(".", StringComparison.Ordinal)..];
		var visitorAssembly = flag.VisitorAssembly ?? typeof(T).Assembly;
		var visitorClass = visitorAssembly.GetType(visitorClassName);
		if (visitorClass == null) {
			throw new InvalidDataException($"Cannot find visitor class {visitorClassName}");
		}

		var visitorMethod = visitorClass.GetMethod(visitorMethodName, BindingFlags.Static);
		if (visitorMethod == null) {
			throw new InvalidDataException($"Cannot find visitor method {visitorMethodName}");
		}

		var parameters = visitorMethod.GetParameters();
		if (visitorMethod.ReturnType.FullName != "System.Void" || parameters.Length != 1 || parameters[0].ParameterType.FullName != "System.String") {
			throw new InvalidDataException($"Visitor method {visitorClassName}.{visitorMethodName} does not match delegate template Func<in string, out object>");
		}

		return visitorMethod.Invoke(null, [textValue]);
	}

	public static string ReconstructArgs(CommandLineFlags inst) {
		var type = inst.GetType();

		var sb = new StringBuilder();

		var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
		var typeMap = properties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).ToDictionary(x => x.x, y => y.Item2);
		var flags = typeMap.Where(x => x.Value?.Hidden == false).ToDictionary(x => x.Key, y => y.Value);

		foreach (var (prop, flag) in flags) {
			if (flag == null || flag.Positional > -1) {
				continue;
			}

			var value = prop.GetValue(inst);
			if (value is null or false) {
				continue;
			}

			sb.Append("--");
			sb.Append(flag.Flag);
			sb.Append(' ');

			var strValue = !prop.PropertyType.IsEnum ? value.ToString()! : ((Enum) value).ToString(prop.PropertyType.GetCustomAttribute<FlagsAttribute>() != null ? "F" : "G");

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
