using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynAnalyser;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CommandHandlerReturnTypeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CH001";

    private const string Title = "Invalid return type";
    private const string MessageFormat = "The method '{0}' in class '{1}' must not return an array type";
    private const string Description = "Methods in a class derived from CommandHandler cannot return array types.";
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null || !classSymbol.IsDerivedFromGeneric("CommandHandler", 2))
        {
            return;
        }

        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IMethodSymbol { Name: "HandleRequestAsync" } methodSymbol ||
                methodSymbol.OverriddenMethod == null)
                continue;

            if (methodSymbol.ReturnType is not INamedTypeSymbol namedReturnType) continue;
            if (namedReturnType.BaseType?.Name != "Task" || namedReturnType.Arity != 1) continue;
            if (namedReturnType.TypeArguments[0] is not IArrayTypeSymbol) continue;
            
            var diagnostic = Diagnostic.Create(
                Rule,
                methodSymbol.Locations[0],
                methodSymbol.Name,
                classSymbol.Name);
            
            context.ReportDiagnostic(diagnostic);
        }
    }
}