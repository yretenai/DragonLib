using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DragonLib.SourceGen;

public class BitPackerSyntaxReceiver : ISyntaxReceiver {
    public HashSet<TypeDeclarationSyntax> TypeDeclarations { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
        if (syntaxNode is TypeDeclarationSyntax { AttributeLists.Count: > 0 } tds) {
            var found = true;
            foreach (var list in tds.AttributeLists) {
                if (list.Attributes.Count == 0) {
                    continue;
                }

                foreach (var attribute in list.Attributes) {
                    if (attribute.Name.ToFullString() == "DragonLib.IO.BitPackedAttribute") {
                        found = true;
                        break;
                    }
                }

                if (found) {
                    break;
                }
            }

            if (!found) {
                return;
            }

            TypeDeclarations.Add(tds);
        }
    }
}
