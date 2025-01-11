using Microsoft.CodeAnalysis;

namespace RoslynAnalyser;

public static class SymbolExtensions
{
    public static bool IsDerivedFromGeneric(this INamedTypeSymbol symbol, string baseTypeName, int genericArity)
    {
        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == baseTypeName && baseType.TypeArguments.Length == genericArity)
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}