using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using RoslynAnalyser;

namespace RoslynAnalyserTests;

[TestFixture]
public class CommandHandlerReturnTypeAnalyzerTests
{
    private const string TestCode = @"
using System.Threading.Tasks;
public abstract class CommandHandler<TIn, TOut> {
    public abstract Task<TOut> HandleRequestAsync(TIn input);
}

public class TestCommandHandler : CommandHandler<string, int[]> {
    public override Task<int[]> HandleRequestAsync(string input) => throw new System.NotImplementedException();
}";

    [Test]
    public async Task Warns_On_Array_Return_Type()
    {
        var expectedDiagnostic = new DiagnosticResult(CommandHandlerReturnTypeAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(8, 33, 8, 46)
            .WithMessage("The method 'HandleRequestAsync' in class 'TestCommandHandler' must not return an array type");

        var test = new CSharpAnalyzerTest<CommandHandlerReturnTypeAnalyzer, DefaultVerifier>
        {
            TestCode = TestCode
        };

        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }
}