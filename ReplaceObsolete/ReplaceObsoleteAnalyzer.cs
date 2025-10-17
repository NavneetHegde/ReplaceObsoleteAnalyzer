using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ReplaceObsolete;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReplaceObsoleteAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ROB001";
    private const string Category = "Maintainability";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));


    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);


    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeIdentifier, SyntaxKind.IdentifierName);
    }

    private static void AnalyzeIdentifier(SyntaxNodeAnalysisContext context)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(context.Node);
        var symbol = symbolInfo.Symbol;

        if (symbol is null)
            return;

        // Look for [Obsolete]
        foreach (var attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute")
            {
                // Extract the message string from [Obsolete("Use NewProp instead")]
                var message = attr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;

                // Pass both arguments for {0} and {1}
                var diagnostic = Diagnostic.Create(
                    Rule,
                    context.Node.GetLocation(),
                    symbol.Name,
                    message);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

}
