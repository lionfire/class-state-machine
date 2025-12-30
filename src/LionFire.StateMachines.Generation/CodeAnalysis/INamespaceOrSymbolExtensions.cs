using Microsoft.CodeAnalysis;
using System.Text;

namespace LionFire.StateMachines.Class.Generation;

// Retrieved from https://stackoverflow.com/a/27106959/208304
internal static class SymbolExtensions
{
    public static string GetFullMetadataName(this INamespaceOrTypeSymbol symbol)
    {
        ISymbol s = symbol;
        if (s == null || IsRootNamespace(s))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(s.MetadataName);
        var last = s;

        s = s.ContainingSymbol;

        while (!IsRootNamespace(s))
        {
            if (s is ITypeSymbol && last is ITypeSymbol)
            {
                sb.Insert(0, '+');
            }
            else
            {
                sb.Insert(0, '.');
            }

            sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            s = s.ContainingSymbol;
        }

        return sb.ToString();
    }

    private static bool IsRootNamespace(ISymbol symbol)
    {
        return symbol is INamespaceSymbol ns && ns.IsGlobalNamespace;
    }
}
