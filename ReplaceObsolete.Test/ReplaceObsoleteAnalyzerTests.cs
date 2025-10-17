using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    ReplaceObsolete.ReplaceObsoleteAnalyzer,
    ReplaceObsolete.ReplaceObsoleteCodeFixProvider>;

namespace ReplaceObsolete.Tests
{
    public class ReplaceObsoleteAnalyzerTests
    {
        [Fact]
        public async Task When_ObsoleteProperty_Should_ReportDiagnostic()
        {
            var testCode = @"
    using System;

    public class MyClass
    {
        [Obsolete(""Use NewProp instead"")]
        public string OldProp { get; set; }

        public string NewProp { get; set; }

        public void Test()
        {
            var x = new MyClass();
            var y = x.[|OldProp|];
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task When_CodeFix_Applied_Should_ReplaceWithNewProperty()
        {
            var testCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }

    public void Test()
    {
        var x = new MyClass();
        var y = x.OldProp;
    }
}";

            var expectedDiagnostic = VerifyCS.Diagnostic("ROB001")
                .WithSpan(14, 19, 14, 26)   // location of 'OldProp'
                .WithArguments("OldProp", "Use NewProp instead");

            var fixedCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }

    public void Test()
    {
        var x = new MyClass();
        var y = x.NewProp;
    }
}";

            // Verify that the analyzer reports a diagnostic at OldProp,
            // and after the code fix, the usage is replaced correctly
            await VerifyCS.VerifyCodeFixAsync(testCode, expectedDiagnostic, fixedCode);
        }

        [Fact]
        public async Task When_ThisQualifiedAccess_Should_ReportDiagnostic_And_Fix()
        {
            var testCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }

    public void Test()
    {
        var y = this.OldProp;
    }
}";
            var expectedDiagnostic = VerifyCS.Diagnostic("ROB001")
                .WithSpan(13, 22, 13, 29)
                .WithArguments("OldProp", "Use NewProp instead");

            var fixedCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }

    public void Test()
    {
        var y = this.NewProp;
    }
}";
            await VerifyCS.VerifyCodeFixAsync(testCode, expectedDiagnostic, fixedCode);
        }

        [Fact]
        public async Task When_MultipleUsages_Should_ReportDiagnostics_And_FixAll()
        {
            var testCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }

    public void Test()
    {
        var a = OldProp;
        var b = this.OldProp;
        var c = NewProp;
    }
}";
            var expectedDiagnostics = new[]
            {
                VerifyCS.Diagnostic("ROB001").WithSpan(13, 17, 13, 24).WithArguments("OldProp", "Use NewProp instead"),
                VerifyCS.Diagnostic("ROB001").WithSpan(14, 22, 14, 29).WithArguments("OldProp", "Use NewProp instead"),
            };

            var fixedCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }

    public void Test()
    {
        var a = NewProp;
        var b = this.NewProp;
        var c = NewProp;
    }
}";
            await VerifyCS.VerifyCodeFixAsync(testCode, expectedDiagnostics, fixedCode);
        }

        [Fact]
        public async Task When_UsedInObjectInitializer_Should_ReportDiagnostic_And_Fix()
        {
            var testCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }
}

public class TestClass
{
    public void Test()
    {
        var obj = new MyClass { OldProp = ""value"" };
    }
}";
            var expectedDiagnostic = VerifyCS.Diagnostic("ROB001")
                .WithSpan(16, 33, 16, 40)
                .WithArguments("OldProp", "Use NewProp instead");

            var fixedCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }
}

public class TestClass
{
    public void Test()
    {
        var obj = new MyClass { NewProp = ""value"" };
    }
}";
            await VerifyCS.VerifyCodeFixAsync(testCode, expectedDiagnostic, fixedCode);
        }

        [Fact]
        public async Task When_StaticObsoleteProperty_Should_ReportDiagnostic_And_Fix()
        {
            var testCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public static string OldProp { get; set; }

    public static string NewProp { get; set; }

    public void Test()
    {
        var y = MyClass.OldProp;
    }
}";
            var expectedDiagnostic = VerifyCS.Diagnostic("ROB001")
                .WithSpan(13, 25, 13, 32)
                .WithArguments("OldProp", "Use NewProp instead");

            var fixedCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public static string OldProp { get; set; }

    public static string NewProp { get; set; }

    public void Test()
    {
        var y = MyClass.NewProp;
    }
}";
            await VerifyCS.VerifyCodeFixAsync(testCode, expectedDiagnostic, fixedCode);
        }

        [Fact]
        public async Task When_UsingNewProperty_Should_NotReportDiagnostic()
        {
            var testCode = @"
using System;

public class MyClass
{
    [Obsolete(""Use NewProp instead"")]
    public string OldProp { get; set; }

    public string NewProp { get; set; }

    public void Test()
    {
        var y = NewProp;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}

