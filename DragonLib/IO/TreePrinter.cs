namespace DragonLib.IO;

public static class TreePrinter {
	private const char Junction = '├';
	private const char Corner = '└';
	private const char Horizontal = '─';
	private const char Vertical = '│';
	private const char Space = ' ';

	public static void PrintTree(TextWriter writer, List<TreeNode> nodes) {
		var count = nodes.Count;
		for (var i = 0; i < count; i++) {
			var node = nodes[i];
			PrintNode(writer, node, string.Empty);

			if (i < count - 1) {
				writer.WriteLine();
			}
		}
	}

	private static void PrintNode(TextWriter writer, TreeNode node, string indent) {
		writer.WriteLine((string.IsNullOrEmpty(indent) ? string.Empty : " ") + node.Name);
		var count = node.Children.Count;
		for (var i = 0; i < count; i++) {
			PrintChildNode(writer, node.Children[i], indent, i == count - 1);
		}
	}

	private static void PrintChildNode(TextWriter writer, TreeNode node, string indent, bool isLast) {
		writer.Write(indent);

		if (isLast) {
			writer.Write($"{Corner}{Horizontal}{Horizontal}");
			indent += $"{Space}{Space}{Space}{Space}";
		} else {
			writer.Write($"{Junction}{Horizontal}{Horizontal}");
			indent += $"{Vertical}{Space}{Space}{Space}";
		}

		PrintNode(writer, node, indent);
	}

	public readonly record struct TreeNode(string Name, List<TreeNode> Children);
}
