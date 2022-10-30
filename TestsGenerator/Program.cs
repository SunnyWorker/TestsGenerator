// See https://aka.ms/new-console-template for more information

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator;



var rewriter = new CodeRewriter();
var tree = CSharpSyntaxTree.ParseText(File.ReadAllText("D:\\Unik\\СПП\\TestsGenerator\\TestsGenerator\\SampleClass.cs"));
var root = tree.GetRoot();
var node = rewriter.GenerateTestClass(root);
Console.WriteLine(node);

