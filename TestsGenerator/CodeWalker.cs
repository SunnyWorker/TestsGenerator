using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator;

public class CodeWalker : CSharpSyntaxWalker
{
    public string fileNamespace;
    public List<ClassDeclarationSyntax> Classes
    {
        get;
    }
    public CodeWalker() : base(SyntaxWalkerDepth.Token)
    {
        fileNamespace = "";
        Classes = new();
    }

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        fileNamespace = node.Name + ".Tests";
        base.VisitFileScopedNamespaceDeclaration(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        Classes.Add(node);
        base.VisitClassDeclaration(node);
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        fileNamespace = node.Name.ToString();
        base.VisitNamespaceDeclaration(node);
    }
}