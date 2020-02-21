using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using DragonLib.IO;
using JetBrains.Annotations;
using OpenTK;

namespace DragonLib.CLI
{
    [PublicAPI]
    public static class CommandLineFlags
    {
        public static void PrintHelp(List<(CLIFlagAttribute? Flag, Type FlagType)> flags)
        {
            flags = flags.Where(x => x.Flag?.Hidden == false).ToList();
            var grouped = flags.GroupBy(x => x.Flag?.Category ?? string.Empty).Select(x => (x.Key, x.ToArray())).ToArray();

            var entry = Assembly.GetEntryAssembly()?.GetName();

            var usageSlim = "Usage: ";
            if (entry != null) usageSlim += $"{entry.Name} ";

            var sizes = new[] { 0, 0, 0 };

            var usageSlimOneCh = "-";
            var usageSlimOneChValue = "";
            var usageSlimOneChValueOptional = "";
            var usageSlimOneChOptional = "[-";
            var usageSlimMultiCh = "";
            var usageSlimPositional = "";

            foreach (var (flag, type) in flags)
            {
                if (flag == null) continue;
                var tn = type.Name;
                if (type.IsConstructedGenericType)
                {
                    var parameters = type.GetGenericArguments().Select(x => x.Name);
                    tn = type.Name.Substring(0, type.Name.IndexOf('`')) + $"<{string.Join(", ", parameters)}>";
                }

                if (flag.Positional > -1) sizes[2] = Math.Max(sizes[2], $"Positional {flag.Positional + 1}".Length);
                sizes[1] = Math.Max(sizes[1], tn.Length);

                var hasValue = type.FullName != "System.Boolean";
                var flagStr = flag.Flag;
                if (flag.Positional == -1) flagStr = string.Join(", ", flag.Flags.Select(sw => $"-{(sw.Length > 1 ? "-" : "")}{sw}{(hasValue ? " value" : "")}"));
                sizes[0] = Math.Max(sizes[0], flagStr.Length);

                if (flag.Positional == -1)
                {
                    var flagOne = flag.Flags.FirstOrDefault(x => x.Length == 1);
                    var flagTwo = flag.Flags.FirstOrDefault(x => x.Length > 1);

                    if (type.FullName == "System.Boolean")
                    {
                        if (flag.IsRequired)
                        {
                            if (flagOne != null) usageSlimOneCh += flagOne;

                            if (flagTwo != null) usageSlimMultiCh += $"--{flagTwo} ";
                        }
                        else
                        {
                            if (flagOne != null) usageSlimOneChOptional += flagOne;

                            if (flagTwo != null) usageSlimMultiCh += $"[--{flagTwo}] ";
                        }
                    }
                    else
                    {
                        if (flag.IsRequired)
                        {
                            if (flagOne != null) usageSlimOneChValue += $"-{flagOne} value ";

                            if (flagTwo != null) usageSlimMultiCh += $"--{flagTwo} value ";
                        }
                        else
                        {
                            if (flagOne != null) usageSlimOneChValueOptional += $"[-{flagOne} value] ";

                            if (flagTwo != null) usageSlimMultiCh += $"[--{flagTwo} value] ";
                        }
                    }
                }
                else
                {
                    var positional = flag.Flag;
                    if (type.IsEquivalentTo(typeof(List<string>)) || type.IsEquivalentTo(typeof(HashSet<string>))) positional += "...";

                    if (flag.IsRequired)
                        usageSlimPositional += $"<{positional}> ";
                    else
                        usageSlimPositional += $"[{positional}] ";
                }
            }

            if (usageSlimOneCh.Length > 1) usageSlim += usageSlimOneCh + " ";

            if (usageSlimOneChOptional.Length > 2) usageSlim += usageSlimOneChOptional + "] ";

            if (usageSlimOneChValue.Length > 1) usageSlim += usageSlimOneChValue;

            if (usageSlimOneChValueOptional.Length > 1) usageSlim += usageSlimOneChValueOptional;

            if (usageSlimMultiCh.Length > 1) usageSlim += usageSlimMultiCh;

            if (usageSlimPositional.Length > 1) usageSlim += usageSlimPositional;

            Logger.Info("FLAG", usageSlim.TrimEnd());
            Logger.Info("FLAG", string.Empty);

            foreach (var (group, attributes) in grouped)
            {
                Logger.Info("FLAG", $"{group}:");

                foreach (var (flag, type) in attributes)
                {
                    if (flag == null) continue;
                    var hasValue = type.FullName != "System.Boolean";

                    var tn = type.Name;
                    if (type.IsConstructedGenericType)
                    {
                        var parameters = type.GetGenericArguments().Select(x => x.Name);
                        tn = type.Name.Substring(0, type.Name.IndexOf('`')) + $"<{string.Join(", ", parameters)}>";
                    }

                    var tp = "";
                    if (flag.Positional > -1) tp += $"Positional {flag.Positional + 1}";
                    tn = tn.PadRight(sizes[1]);
                    tp = tp.PadLeft(sizes[2]);

                    var requiredParts = new List<string>();
                    object? def = null;
                    if (type.IsValueType) def = Activator.CreateInstance(type);

                    if ((flag.Default != null && !flag.Default.Equals(def)) || type.IsEnum) requiredParts.Add($"Default: {flag.Default}");
                    if (flag.IsRequired) requiredParts.Add("Required");
                    if (flag.ValidValues?.Length > 0) requiredParts.Add("Values: " + string.Join(", ", flag.ValidValues));
                    else if (type.IsEnum) requiredParts.Add("Values: " + string.Join(", ", Enum.GetNames(type)));

                    var required = string.Join(", ", requiredParts);
                    if (required.Length > 0) required = $" ({required})";

                    var flagStr = flag.Flag;
                    if (flag.Positional == -1) flagStr = string.Join(", ", flag.Flags.Select(sw => $"-{(sw.Length > 1 ? "-" : "")}{sw}{(hasValue ? " value" : "")}"));
                    Logger.Info("FLAG", flagStr.PadRight(sizes[0]) + "\t" + tn + "\t" + tp + "\t" + flag.Help + required);
                }

                Logger.Info("FLAG", string.Empty);
            }
        }

        public static T? ParseFlags<T>(Action<List<(CLIFlagAttribute?, Type)>> printHelp, params string[] arguments) where T : ICLIFlags
        {
            var instance = Activator.CreateInstance<T>();

            var typeMap = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty).Select(x => (x, x.GetCustomAttribute<CLIFlagAttribute>())).Where(x => x.Item2 != null).ToDictionary(x => x.x, y => (y.Item2, y.x.PropertyType));

            var argMap = new Dictionary<string, int>();
            var positionalMap = new HashSet<int>();
            for (var index = 0; index < arguments.Length; index++)
            {
                var argument = arguments[index];
                if (argument.StartsWith("-"))
                {
                    if (argument.StartsWith("--"))
                        argMap[argument.Substring(2)] = index;
                    else
                        foreach (var argc in argument.Substring(1))
                            argMap[argc.ToString()] = index;
                }
                else
                {
                    positionalMap.Add(index);
                }
            }

            foreach (var (property, (flag, type)) in typeMap.Where(x => x.Value.Item1?.Positional == -1))
            {
                if (flag == null) continue;
                var index = -1;
                foreach (var sw in flag.Flags)
                {
                    if (argMap.TryGetValue(sw, out index)) break;

                    index = -1;
                }

                if (index == -1 && flag.IsRequired)
                {
                    Logger.Info("FLAG", $"-{(flag.Flag.Length > 1 ? "-" : "")}{flag.Flag} needs a value!");
                    printHelp(typeMap.Values.ToList());
                    Environment.Exit(0);
                    return null;
                }

                var value = flag.Default;

                if (index > -1)
                {
                    if (type.FullName == "System.Boolean")
                    {
                        if (!(value is bool b)) b = false;

                        value = !b;
                    }
                    else
                    {
                        var textValue = arguments[index + 1];
                        if (textValue.StartsWith("-"))
                        {
                            Logger.Error("FLAG", $"-{(flag.Flag.Length > 1 ? "-" : "")}{flag.Flag} needs a value!");
                            printHelp(typeMap.Values.ToList());
                            Environment.Exit(0);
                            return null;
                        }

                        positionalMap.Remove(index + 1);

                        if (VisitFlagValue<T>(printHelp, type, textValue, flag, typeMap, ref value)) return null;
                    }
                }

                property.SetValue(instance, value);
            }

            var positionals = positionalMap.Select(x => arguments[x]).ToList();

            foreach (var (property, (flag, type)) in typeMap.Where(x => x.Value.Item1?.Positional > -1))
            {
                if (flag == null) continue;

                if (flag.IsRequired && flag.Positional >= positionalMap.Count)
                {
                    Logger.Error("FLAG", $"Positional {flag.Flag} needs a value!");
                    printHelp(typeMap.Values.ToList());
                    Environment.Exit(0);
                    return null;
                }

                var value = flag.Default;

                if (type.IsEquivalentTo(typeof(List<string>)))
                    value = positionals.Skip(flag.Positional).ToList();
                else if (type.IsEquivalentTo(typeof(HashSet<string>)))
                    value = positionals.Skip(flag.Positional).ToHashSet();
                else if (positionals.Count > flag.Positional && VisitFlagValue<T>(printHelp, type, positionals[flag.Positional], flag, typeMap, ref value)) return null;

                property.SetValue(instance, value);
            }

            if (!instance.Help) return instance;

            printHelp(typeMap.Values.ToList());
            Environment.Exit(0);
            return null;
        }

        private static bool VisitFlagValue<T>(Action<List<(CLIFlagAttribute?, Type)>> printHelp, Type type, string textValue, CLIFlagAttribute flag, Dictionary<PropertyInfo, (CLIFlagAttribute?, Type PropertyType)> typeMap, ref object? value) where T : ICLIFlags
        {
            if (flag.ValidValues?.Length > 0 && !flag.ValidValues.Contains(textValue))
            {
                Logger.Error("FLAG", $"Unrecognized value {textValue} for -{(flag.Flag.Length > 1 ? "-" : "")}{flag.Flag}! Valid values are {string.Join(", ", flag.ValidValues)}");
                printHelp(typeMap.Values.ToList());
                Environment.Exit(0);
                return true;
            }

            if (type.IsEnum)
            {
                if (!Enum.TryParse(type, textValue, true, out value)) Logger.Error("FLAG", $"Unrecognized value {textValue} for -{(flag.Flag.Length > 1 ? "-" : "")}{flag.Flag}! Valid values are {string.Join(", ", Enum.GetNames(type))}");
            }
            else
            {
                try
                {
                    value = type.FullName switch
                    {
                        "System.Int64" => long.Parse(textValue, NumberStyles.Any),
                        "System.UInt64" => ulong.Parse(textValue, NumberStyles.Any),
                        "System.Int32" => int.Parse(textValue, NumberStyles.Any),
                        "System.UInt32" => uint.Parse(textValue, NumberStyles.Any),
                        "System.Int16" => short.Parse(textValue, NumberStyles.Any),
                        "System.UInt16" => ushort.Parse(textValue, NumberStyles.Any),
                        "System.Byte" => byte.Parse(textValue, NumberStyles.Any),
                        "System.SByte" => sbyte.Parse(textValue, NumberStyles.Any),
                        "System.Double" => double.Parse(textValue),
                        "System.Single" => float.Parse(textValue),
                        "System.String" => textValue,
                        "DragonLib.Numerics.Half" => Half.Parse(textValue),
                        _ => InvokeVisitor<T>(flag, type, textValue)
                    };
                }
                catch (Exception e)
                {
                    Logger.Error("FLAG", $"-{(flag.Flag.Length > 1 ? "-" : "")}{flag.Flag} failed to parse {textValue} as a {type.Name}!", e);
                    printHelp(typeMap.Values.ToList());
                    Environment.Exit(0);
                    return true;
                }
            }

            return false;
        }

        private static object? InvokeVisitor<T>(CLIFlagAttribute flag, Type type, string textValue) where T : ICLIFlags
        {
            if (flag.Visitor == null) throw new InvalidCastException($"Cannot process {type.FullName}");
            var visitorClassName = flag.Visitor.Substring(0, flag.Visitor.LastIndexOf(".", StringComparison.Ordinal));
            var visitorMethodName = flag.Visitor.Substring(flag.Visitor.LastIndexOf(".", StringComparison.Ordinal));
            var visitorAssembly = flag.VisitorAssembly ?? typeof(T).Assembly;
            var visitorClass = visitorAssembly.GetType(visitorClassName);
            if (visitorClass == null) throw new InvalidDataException($"Cannot find visitor class {visitorClassName}");
            var visitorMethod = visitorClass.GetMethod(visitorMethodName, BindingFlags.Static);
            if (visitorMethod == null) throw new InvalidDataException($"Cannot find visitor method {visitorMethodName}");
            var parameters = visitorMethod.GetParameters();
            if (visitorMethod.ReturnType.FullName != "System.Void" || parameters.Length != 1 || parameters[0].ParameterType.FullName != "System.String")
                throw new InvalidDataException($"Visitor method {visitorClassName}.{visitorMethodName} does not match delegate template Func<in string, out object>");
            return visitorMethod.Invoke(null, new object?[] { textValue });
        }
    }
}
