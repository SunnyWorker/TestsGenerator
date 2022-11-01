using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace TestsGenerator
{
    public class CodeWalker : CSharpSyntaxWalker
    {
        
        public string currentNamespace = "";
        public ConcurrentDictionary<string, ConcurrentDictionary<string,List<string>>> Classes
        {
            get;
        }

        public CodeWalker() : base(SyntaxWalkerDepth.Token)
        {
            Classes = new();
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            currentNamespace = node.Name.ToString();
            Classes.TryAdd(currentNamespace,new ());
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            currentNamespace = node.Name.ToString();
            Classes.TryAdd(currentNamespace,new ());
            base.VisitFileScopedNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (currentNamespace.Equals("") && !Classes.ContainsKey(currentNamespace))
                Classes.TryAdd(currentNamespace, new()); 
            Classes[currentNamespace][node.Identifier.Text] = new();
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
                Classes[currentNamespace][
                        ((ClassDeclarationSyntax)node.Parent).Identifier.ToString()].Add(node.Identifier.ToString());
            }
            base.VisitMethodDeclaration(node);
        }
    }
}