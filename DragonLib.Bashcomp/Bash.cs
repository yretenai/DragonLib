using System.Text;

namespace DragonLib.Bashcomp;

public class Bash : ITemplate {
	public string Generate(string name, HashSet<Option> options) {
		var sb = new StringBuilder();
		sb.AppendLine($"_{name}()");
		sb.AppendLine("{");
		sb.AppendLine("\tlocal cur=${COMP_WORDS[COMP_CWORD]}");
		sb.AppendLine("\tlocal opts=\"");

		foreach (var option in options) {
			foreach (var flag in option.Flags) {
				sb.Append('-');
				if (flag.Length > 1) {
					sb.Append('-');
				}

				sb.Append($"{flag} ");
			}

			sb.AppendLine("\\");
		}

		sb.AppendLine("\"");
		sb.AppendLine("\tif [[ ${cur} == -* ]]; then");
		sb.AppendLine("\t\tCOMPREPLY=($(compgen -W \"${opts}\" -- \"${cur}\"))");
		sb.AppendLine("\telse");
		sb.AppendLine("\t\t_filedir");
		sb.AppendLine("\tfi");
		sb.AppendLine("} &&");
		sb.AppendLine($"complete -o default -F _{name} {name}");
		return sb.ToString();
	}
}
