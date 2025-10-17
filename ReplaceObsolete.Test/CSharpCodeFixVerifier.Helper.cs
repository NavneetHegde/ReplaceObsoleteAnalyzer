using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading.Tasks;  // Contains XunitVerifier

namespace ReplaceObsolete.Tests
{
    public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public static async Task VerifyAnalyzerAsync(string source)
        {
            var test = new Test { TestCode = source };
            await test.RunAsync();
        }

        public static async Task VerifyCodeFixAsync(string source, string fixedSource)
        {
            var test = new Test
            {
                TestCode = source,
                FixedCode = fixedSource
            };

            await test.RunAsync();
        }

        public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
        {
        }
    }
}
