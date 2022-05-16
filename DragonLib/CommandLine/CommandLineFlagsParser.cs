using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DragonLib.IO;

namespace DragonLib.CommandLine;

public static class CommandLineFlagsParser {
    public delegate void PrintHelpDelegate(List<(FlagAttribute? Flag, Type FlagType)> flags, bool helpInvoked, ILogger logger);

    public static void PrintHelp<T>(PrintHelpDelegate printHelp, bool helpInvoked, ILogger logger) {
        PrintHelp(typeof(T), printHelp, helpInvoked, logger);
    }

    public static void PrintHelp(Type t, PrintHelpDelegate printHelp, bool helpInvoked, ILogger logger) {
        var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        var typeMap = properties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).ToDictionary(x => x.x, y => (y.Item2, y.x.PropertyType));
        var propertyNameToProperty = properties.ToDictionary(x => x.Name, y => y);
        foreach (var @interface in t.GetInterfaces()) {
            var interfaceProperties = @interface.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var (prop, info) in interfaceProperties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).Where(x => x.Item2 != null)) {
                if (!propertyNameToProperty.TryGetValue(prop.Name, out var propertyImplementation) || !typeMap.TryGetValue(propertyImplementation, out var propertySet) || propertySet.Item1?.Equals(default) == false) {
                    continue;
                }

                propertySet.Item1 = info;
                typeMap[propertyImplementation] = propertySet;
            }
        }

        typeMap = typeMap.Where(x => x.Value.Item1?.Equals(default) == false).ToDictionary(x => x.Key, y => y.Value);
        printHelp(typeMap.Values.ToList(), helpInvoked, logger);
    }

    public static void PrintHelp(List<(FlagAttribute? Flag, Type FlagType)> flags, bool helpInvoked, ILogger logger) {
        flags = flags.Where(x => x.Flag?.Hidden == false).ToList();
        var grouped = flags.GroupBy(x => x.Flag?.Category ?? string.Empty).Select(x => (x.Key, x.ToArray())).ToArray();
        var entry = Assembly.GetEntryAssembly()?.GetName();
        var usageSlim = "Usage: ";
        if (entry != null) {
            usageSlim += $"{entry.Name} ";
        }

        var sizes = new[] { 0, 0, 0 };
        var usageSlimOneCh = "-";
        var usageSlimOneChValue = string.Empty;
        var usageSlimOneChValueOptional = string.Empty;
        var usageSlimOneChOptional = "[-";
        var usageSlimMultiCh = string.Empty;
        var usageSlimPositional = string.Empty;
        foreach (var (flag, originalType) in flags) {
            if (flag == null) {
                continue;
            }

            var type = originalType;
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                type = type.GetGenericArguments()[0];
            }

            var tn = type.Name;
            if (type.IsConstructedGenericType) {
                var parameters = type.GetGenericArguments().Select(x => x.Name);
                tn = type.Name[..type.Name.IndexOf('`')] + $"<{string.Join(", ", parameters)}>";
            }

            if (flag.Positional > -1) {
                sizes[2] = Math.Max(sizes[2], $"Positional {flag.Positional + 1}".Length);
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

        logger.Log(LogLevel.Information, usageSlim.TrimEnd());
        logger.Log(LogLevel.Information, string.Empty);
        foreach (var (group, attributes) in grouped) {
            logger.Log(LogLevel.Information, "{Group}: ", group);
            foreach (var (flag, originalType) in attributes) {
                if (flag == null) {
                    continue;
                }

                var type = originalType;
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    type = type.GetGenericArguments()[0];
                }

                var hasValue = type.FullName != "System.Boolean";
                var tn = type.Name;
                if (type.IsConstructedGenericType) {
                    var parameters = type.GetGenericArguments().Select(x => x.Name);
                    tn = type.Name[..type.Name.IndexOf('`')] + $"<{string.Join(", ", parameters)}>";
                }

                var tp = string.Empty;
                if (flag.Positional > -1) {
                    tp += $"Positional {flag.Positional + 1}";
                }

                tn = tn.PadRight(sizes[1]);
                tp = tp.PadLeft(sizes[2]);
                var requiredParts = new List<string>();
                object? def = null;
                if (type.IsValueType) {
                    def = Activator.CreateInstance(type);
                }

                if (flag.Default != null && !flag.Default.Equals(def) || type.IsEnum) {
                    requiredParts.Add($"Default: {flag.Default}");
                }

                if (flag.IsRequired) {
                    requiredParts.Add("Required");
                }

                if (flag.ValidValues?.Length > 0) {
                    requiredParts.Add("Values: " + string.Join(", ", flag.ValidValues));
                } else if (type.IsEnum) {
                    var names = Enum.GetNames(type);
                    if (!string.IsNullOrEmpty(flag.EnumPrefix)) {
                        var start = flag.EnumPrefix.Length;
                        names = names.Select(x => x.StartsWith(flag.EnumPrefix) ? x[start..] : x).ToArray();
                    }

                    requiredParts.Add("Values: " + string.Join(", ", helpInvoked ? names : names.Take(3)));
                    if (!helpInvoked && names.Length > 3) {
                        requiredParts[^1] += $", and {names.Length - 3} more";
                    }
                }

                var required = string.Join(", ", requiredParts);
                if (required.Length > 0) {
                    required = $" ({required})";
                }

                var flagStr = flag.Flag;
                if (flag.Positional == -1) {
                    flagStr = string.Join(", ", flag.Flags.Select(sw => $"-{(sw.Length > 1 ? "-" : string.Empty)}{sw}{(hasValue ? " value" : string.Empty)}"));
                }

                logger.Log(LogLevel.Information, "{Pad}\t{Name}\t{Pos}\t{Help}\t{Required}", flagStr.PadRight(sizes[0]), tn, tp, flag.Help, required);
            }

            logger.Log(LogLevel.Information, string.Empty);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ParseFlags<T>(ILogger logger) where T : CommandLineFlags => ParseFlags<T>(logger, Environment.GetCommandLineArgs().Skip(1).ToArray());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ParseFlags<T>(ILogger logger, params string[] arguments) where T : CommandLineFlags => ParseFlags<T>(logger, PrintHelp, arguments);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandLineFlags? ParseFlags(ILogger logger, Type t) => typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), Array.Empty<Type>())?.MakeGenericMethod(t).Invoke(null, new object?[] { logger }) as CommandLineFlags;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandLineFlags? ParseFlags(ILogger logger, Type t, params string[] arguments) {
        return typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), new[] { typeof(string[]) })?.MakeGenericMethod(t).Invoke(null, new object?[] { logger, arguments }) as CommandLineFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandLineFlags? ParseFlags(ILogger logger, Type t, PrintHelpDelegate printHelp, params string[] arguments) {
        return typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), new[] { typeof(PrintHelpDelegate), typeof(string[]) })?.MakeGenericMethod(t).Invoke(null, new object?[] { logger, printHelp, arguments }) as CommandLineFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandLineFlags? ParseFlags(ILogger logger, Type t, PrintHelpDelegate printHelp) {
        return typeof(CommandLineFlagsParser).GetMethod(nameof(ParseFlags), new[] { typeof(PrintHelpDelegate) })?.MakeGenericMethod(t).Invoke(null, new object?[] { logger, printHelp }) as CommandLineFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ParseFlags<T>(ILogger logger, PrintHelpDelegate printHelp) where T : CommandLineFlags => ParseFlags<T>(logger, printHelp, Environment.GetCommandLineArgs().Skip(1).ToArray());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ParseFlags<T>(ILogger logger, PrintHelpDelegate printHelp, params string[] arguments) where T : CommandLineFlags {
        var shouldExit = false;
        var instance = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        var typeMap = properties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).ToDictionary(x => x.x, y => (y.Item2, y.x.PropertyType));
        var propertyNameToProperty = properties.ToDictionary(x => x.Name, y => y);
        foreach (var @interface in typeof(T).GetInterfaces()) {
            var interfaceProperties = @interface.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var (prop, info) in interfaceProperties.Select(x => (x, x.GetCustomAttribute<FlagAttribute>(true))).Where(x => x.Item2 != null)) {
                if (!propertyNameToProperty.TryGetValue(prop.Name, out var propertyImplementation) || !typeMap.TryGetValue(propertyImplementation, out var propertySet) || propertySet.Item1?.Equals(default) == false) {
                    continue;
                }

                propertySet.Item1 = info;
                typeMap[propertyImplementation] = propertySet;
            }
        }

        typeMap = typeMap.Where(x => x.Value.Item1?.Equals(default) == false).ToDictionary(x => x.Key, y => y.Value);
        var argMap = new Dictionary<string, HashSet<int>>();
        var positionalMap = new HashSet<int>();
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
                positionalMap.Add(index);
            }
        }

        if (propertyNameToProperty.TryGetValue("Help", out var helpProperty) && typeMap.TryGetValue(helpProperty, out var helpEntry)) {
            if (helpEntry.Item1 != null && helpEntry.Item1.Flags.Any(flag => argMap.ContainsKey(flag))) {
                printHelp(typeMap.Values.ToList(), true, logger);
                Environment.Exit(0);
                return null;
            }
        }

        foreach (var (property, (flag, originalType)) in typeMap.Where(x => x.Value.Item1?.Positional == -1)) {
            if (flag == null) {
                continue;
            }

            var type = originalType;
            var isNullable = type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable) {
                type = type.GetGenericArguments()[0];
            }

            var indexList = default(HashSet<int>);
            foreach (var sw in flag.Flags) {
                if (argMap.TryGetValue(sw, out indexList)) {
                    break;
                }
            }

            if (indexList == null && flag.IsRequired) {
                logger.Log(LogLevel.Error, "{Flag} needs a value", flag.Flag);
                shouldExit = true;
            }

            var value = flag.Default;
            if (indexList != null) {
                foreach (var index in indexList) {
                    if (type.FullName == "System.Boolean") {
                        if (value is not bool b) {
                            b = false;
                        }

                        value = !b;
                    } else {
                        var argument = arguments[index];
                        var hasInnateValue = argument.Contains('=');
                        string textValue;
                        if (hasInnateValue) {
                            textValue = argument[(argument.IndexOf('=') + 1)..];
                            if (string.IsNullOrWhiteSpace(textValue)) {
                                logger.Log(LogLevel.Error, "{Flag} needs a value", flag.Flag);
                                shouldExit = true;
                            }
                        } else {
                            if (!positionalMap.Contains(index + 1)) {
                                logger.Log(LogLevel.Error, "{Flag} needs a value", flag.Flag);
                                shouldExit = true;
                            }

                            textValue = arguments[index + 1];
                            positionalMap.Remove(index + 1);
                        }

                        if (type.IsConstructedGenericType && (type.GetGenericTypeDefinition().IsEquivalentTo(typeof(List<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(HashSet<>)))) {
                            var temp = default(object?);
                            if (VisitFlagValue<T>(type.GetGenericArguments()[0], textValue, flag, ref temp, logger)) {
                                return null;
                            }

                            value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
                            type.GetMethod("Add")?.Invoke(value, new[] { temp });
                        } else if (VisitFlagValue<T>(type, textValue, flag, ref value, logger)) {
                            return null;
                        }
                    }
                }
            }

            if (isNullable) {
                value = Activator.CreateInstance(originalType, value);
            }

            try {
                value ??= Activator.CreateInstance(type);
            } catch {
                // ignored
            }

            property.SetValue(instance, value);
        }

        var positionals = positionalMap.Select(x => arguments[x]).ToList();
        foreach (var (property, (flag, originalType)) in typeMap.Where(x => x.Value.Item1?.Positional > -1)) {
            if (flag == null) {
                continue;
            }

            var type = originalType;
            var isNullable = type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable) {
                type = type.GetGenericArguments()[0];
            }

            if (flag.IsRequired && flag.Positional >= positionalMap.Count) {
                logger.Log(LogLevel.Error, "Positional {Flag} needs a value", flag.Flag);
                shouldExit = true;
            }

            var value = flag.Default;
            if (type.IsConstructedGenericType && (type.GetGenericTypeDefinition().IsEquivalentTo(typeof(List<>)) || type.GetGenericTypeDefinition().IsEquivalentTo(typeof(HashSet<>)))) {
                var temp = default(object?);
                value = property.GetValue(instance) ?? value ?? Activator.CreateInstance(type);
                foreach (var textValue in positionals.Skip(flag.Positional)) {
                    if (VisitFlagValue<T>(type.GetGenericArguments()[0], textValue, flag, ref temp, logger)) {
                        return null;
                    }

                    type.GetMethod("Add")?.Invoke(value, new[] { temp });
                }
            } else if (positionals.Count > flag.Positional && VisitFlagValue<T>(type, positionals[flag.Positional], flag, ref value, logger)) {
                shouldExit = true;
            }

            if (isNullable) {
                value = Activator.CreateInstance(originalType, value);
            }

            try {
                value ??= Activator.CreateInstance(type);
            } catch {
                // ignored
            }

            property.SetValue(instance, value);
        }

        if (!instance.Help && !shouldExit) {
            return instance;
        }

        printHelp(typeMap.Values.ToList(), instance.Help, logger);
        Environment.Exit(0);
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool VisitFlagValue<T>(Type type, string textValue, FlagAttribute flag, ref object? value, ILogger logger) where T : CommandLineFlags {
        var sterilizedValue = textValue;
        if (flag.ReplaceDashes > 0) {
            sterilizedValue = sterilizedValue.Replace('-', flag.ReplaceDashes);
        }

        if (flag.ReplaceDots > 0) {
            sterilizedValue = sterilizedValue.Replace('.', flag.ReplaceDashes);
        }

        if (flag.ValidValues?.Length > 0 && !flag.ValidValues.Contains(sterilizedValue)) {
            logger.Log(LogLevel.Error, "Unrecognized value {Value} for {Flag}! Valid values are {Valid}", sterilizedValue, flag.Flag, string.Join(", ", flag.ValidValues));
            return true;
        }

        if (type.IsEnum) {
            if (!Enum.TryParse(type, sterilizedValue, true, out value) && !Enum.TryParse(type, flag.EnumPrefix + sterilizedValue, out value)) {
                logger.Log(LogLevel.Error, "Unrecognized value {Value} for {Flag}! Valid values are {Valid}", textValue, flag.Flag, string.Join(", ", Enum.GetNames(type)));
            }
        } else {
            try {
                value = type.FullName switch {
                    "System.Int64" => long.Parse(textValue, NumberStyles.Any),
                    "System.UInt64" => ulong.Parse(textValue, NumberStyles.HexNumber),
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
                    _ => InvokeVisitor<T>(flag, type, textValue),
                };
            } catch (Exception e) {
                logger.Log(e, "{Flag} failed to parse {Value} as a {Type}", flag.Flag, textValue, type.Name);
                return true;
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

        return visitorMethod.Invoke(null, new object?[] { textValue });
    }
}
