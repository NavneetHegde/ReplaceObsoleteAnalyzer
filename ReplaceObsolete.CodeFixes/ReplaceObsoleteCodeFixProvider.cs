using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReplaceObsolete;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceObsoleteCodeFixProvider)), Shared]
public class ReplaceObsoleteCodeFixProvider : CodeFixProvider
{
    private const string Title = "Replace obsolete property usage";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(ReplaceObsoleteAnalyzer.DiagnosticId);


    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var diagnostic = context.Diagnostics.First();
        var node = root?.FindNode(diagnostic.Location.SourceSpan);

        if (node is IdentifierNameSyntax identifierName)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var symbol = semanticModel.GetSymbolInfo(identifierName).Symbol;

            // Look for ObsoleteAttribute with "Use X instead"
            var obsoleteAttr = symbol?.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute");

            var message = obsoleteAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString();

            string replacement = null;
            if (!string.IsNullOrWhiteSpace(message) && message!.StartsWith("Use "))
            {
                // Example: "Use New_Aborted instead"
                replacement = message.Split(' ')[1];
            }

            if (replacement != null)
            {
                context.RegisterCodeFix(
                    Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                        $"Replace with '{replacement}'",
                        c => ReplaceAsync(context.Document, identifierName, replacement, c),
                        nameof(ReplaceObsoleteCodeFixProvider)),
                    diagnostic);
            }
        }
    }

    private async Task<Document> ReplaceAsync(Document document, IdentifierNameSyntax oldNode, string newName, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        var newNode = oldNode.WithIdentifier(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(newName));
        editor.ReplaceNode(oldNode, newNode);
        return editor.GetChangedDocument();
    }
}
