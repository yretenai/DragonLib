using Microsoft.CodeAnalysis;

namespace DragonLib.SourceGen;

public class BitPackerSourceGen : ISourceGenerator {
    public void Initialize(GeneratorInitializationContext context) {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
            SpinWait.SpinUntil(() => Debugger.IsAttached);
        }
#endif
        context.RegisterForSyntaxNotifications(() => new BitPackerSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context) {
        var compilation = context.Compilation;
        var bpsr = (BitPackerSyntaxReceiver) context.SyntaxReceiver!;

        foreach (var target in bpsr.TypeDeclarations) {
            var semanticModel = compilation.GetSemanticModel(target.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(target);
            var name = GetFullQualifiedName(symbol!);
            // todo: get parameters with the BitFieldAttribute namespace.
        }
    }

    private static string GetFullQualifiedName(ISymbol symbol) {
        var containingNamespace = symbol.ContainingNamespace;
        if (!containingNamespace.IsGlobalNamespace) {
            return containingNamespace.ToDisplayString() + "." + symbol.Name;
        }

        return symbol.Name;
    }
}
