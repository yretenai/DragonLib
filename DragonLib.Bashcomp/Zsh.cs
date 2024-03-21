using System.Text;

namespace DragonLib.Bashcomp;

public class Zsh : ITemplate {
	public string Generate(string name, HashSet<Option> options) {
		var sb = new StringBuilder();
		sb.AppendLine($"#compdef {name}");
		sb.AppendLine();

		foreach (var enumOption in options.OfType<EnumOption>().OrderBy(x => x.Name)) {
			sb.AppendLine($"_{enumOption.Name}() {{");
			sb.AppendLine("\t_values \\");
			sb.Append($"\t\t'{enumOption.Name.ToUpperInvariant()}'");
			foreach (var enumValue in enumOption.Values) {
				sb.AppendLine(" \\");
				sb.Append($"\t\t'{enumValue.Name}[{enumValue.Help.Quoted('\'', wrapQuotes: false)}]'");
			}

			sb.AppendLine("}");
			sb.AppendLine();
		}

		sb.AppendLine("_arguments \\");

		foreach (var option in options.OrderBy(x => x.Flags[0])) {
			if (option.Flags.Count == 1) {
				sb.Append("\t'-");
				var flag = option.Flags[0];
				if (flag.Length > 1) {
					sb.Append('-');
				}

				sb.Append($"{flag}'");
			} else {
				sb.Append("\t'(");
				var sorted = option.Flags.OrderBy(x => x.Length).ToArray();
				for (var index = 0; index < option.Flags.Count; index++) {
					var hasNext = index < option.Flags.Count - 1;
					var flag = sorted[index];
					sb.Append('-');
					if (flag.Length > 1) {
						sb.Append('-');
					}

					sb.Append($"{flag}");
					if (hasNext) {
						sb.Append(' ');
					}
				}

				sb.Append(")'{");
				for (var index = 0; index < option.Flags.Count; index++) {
					var hasNext = index < option.Flags.Count - 1;
					var flag = sorted[index];
					sb.Append('-');
					if (flag.Length > 1) {
						sb.Append('-');
					}

					sb.Append($"{flag}");
					if (hasNext) {
						sb.Append(',');
					}
				}

				sb.Append('}');
			}

			sb.Append($"'[{option.Help.Quoted('\'', wrapQuotes: false)}]'");
			if (option is EnumOption enumOption) {
				sb.Append($":{enumOption.Name}:_{enumOption.Name}");
			}

			sb.AppendLine(" \\");
		}

		sb.AppendLine("\t'*:default:_default' \\");
		sb.AppendLine("\t && return 0");
		sb.AppendLine();
		sb.AppendLine("return 1");
		return sb.ToString();
	}
}
