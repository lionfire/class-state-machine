using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace LionFire.StateMachines.Class.Generation
{
    // Retrieved from https://stackoverflow.com/a/61879121/208304 
    public static class SyntaxNodeHelper
    {
        public static string GetPrefix(SyntaxNode member)
        {
            if (member == null)
            {
                return "unknown";
            }

            var sb = new StringBuilder();
            SyntaxNode node = member;

            while (node.Parent != null)
            {
                node = node.Parent;

                if (node is NamespaceDeclarationSyntax)
                {
                    var namespaceDeclaration = (NamespaceDeclarationSyntax)node;

                    sb.Insert(0, ".");
                    sb.Insert(0, namespaceDeclaration.Name.ToString());
                }
                else if (node is ClassDeclarationSyntax)
                {
                    var classDeclaration = (ClassDeclarationSyntax)node;

                    sb.Insert(0, ".");
                    sb.Insert(0, classDeclaration.Identifier.ToString());
                }
            }

            if (sb.Length == 0) sb.Append("blank");

            return sb.ToString();
        }
    }
}
