namespace DragonLib.Bashcomp;

public interface ITemplate {
	public string Generate(string name, HashSet<Option> options);
}
