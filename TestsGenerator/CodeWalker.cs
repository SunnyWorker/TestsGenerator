using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator;

public class CodeWalker : CSharpSyntaxWalker
{
    public string fileNamespace;
    public ConcurrentDictionary<string, List<string>> Methods
    {
        get;
    }

    public CodeWalker() : base(SyntaxWalkerDepth.Token)
    {
        fileNamespace = "";
        Methods = new();
    }

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        fileNamespace = node.Name + ".Tests";
        base.VisitFileScopedNamespaceDeclaration(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        Methods.TryAdd(node.Identifier.ToString(),new ());
        base.VisitClassDeclaration(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        bool agree = false;
        foreach (var nodeModifier in node.Modifiers)
        {
            if (nodeModifier.Text.Equals("public"))
            {
                agree = true;
            }
        }

        if (agree)
        {
            Methods[((ClassDeclarationSyntax)node.Parent).Identifier.ToString()].Add(node.Identifier.ToString());
        }
        base.VisitMethodDeclaration(node);
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        fileNamespace = node.Name.ToString();
        base.VisitNamespaceDeclaration(node);
    }
}