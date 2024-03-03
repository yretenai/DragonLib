using System.ComponentModel;
using System.Reflection;
using DragonLib.Bashcomp;
using DragonLib.CommandLine;

if (args.Length < 2) {
    Console.Error.WriteLine("DragonLib.Bashcomp path/to/program.dll zsh/bash");
}

ITemplate template = args[1].ToLowerInvariant() switch {
                         "zsh"  => new Zsh(),
                         "bash" => new Bash(),
                         _      => throw new NotSupportedException(args[1])
                     };

var asm = Assembly.LoadFrom(args[0]);
var types = asm.GetTypes();
var options = new HashSet<Option>();
foreach (var type in types) {
    var baseType = type.BaseType;
    while (baseType != null) {
        if (baseType.FullName == "DragonLib.CommandLine.CommandLineFlags") {
            break;
        }

        baseType = baseType.BaseType;
    }

    if (baseType == null) {
        continue;
    }

    var flags = CommandLineFlagsParser.GetFlags(type);
    foreach (var (flag, t) in flags.Values) {
        if (flag.Positional > -1) {
            continue;
        }

        if (flag.Hidden) {
            continue;
        }

        if (t.IsEnum) {
            var values = new List<EnumValue>();
            var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields.Skip(1)) {
                var description = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
                values.Add(new EnumValue(field.Name, description));
            }

            options.Add(new EnumOption(t.Name, flag.Help ?? string.Empty, flag.Flags.ToList(), values));
        } else {
            options.Add(new Option(flag.Help ?? string.Empty, flag.Flags.ToList()));
        }
    }
}

Console.WriteLine(template.Generate(Path.GetFileNameWithoutExtension(args[0]), options));
