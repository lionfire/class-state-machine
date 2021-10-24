using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace LionFire.StateMachines.Tests
{
    // From https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md
    //

    //public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    //    where TSourceGenerator : ISourceGenerator, new()
    //{
    //    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, XUnitVerifier>
    //    {
    //        public Test()
    //        {
    //        }

    //        protected override CompilationOptions CreateCompilationOptions()
    //        {
    //            var compilationOptions = base.CreateCompilationOptions();
    //            return compilationOptions.WithSpecificDiagnosticOptions(
    //                 compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
    //        }

    //        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

    //        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
    //        {
    //            string[] args = { "/warnaserror:nullable" };
    //            var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
    //            var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

    //            return nullableWarnings;
    //        }

    //        protected override ParseOptions CreateParseOptions()
    //        {
    //            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
    //        }
    //    }
    //}

    // Then, in your test file:
    // using VerifyCS = CSharpSourceGeneratorVerifier<YourGenerator>;

    //class Test
    //{
    //    void Method()
    //    {
    //        var code = "initial code";
    //        var generated = "expected generated code";
    //        await new VerifyCS.Test
    //        {
    //            TestState =
    //            {
    //                Sources = { code },
    //                GeneratedSources =
    //                {
    //                    (typeof(YourGenerator), "GeneratedFileName", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
    //                },
    //            },
    //        }.RunAsync();
    //    }
    //}

    // Solution B: ...
}