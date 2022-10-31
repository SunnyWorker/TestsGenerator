using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator;

namespace Tests;

public class Tests
{
    [Test]
    public void UsualClassTesting()
    {
        string codeText = File.ReadAllText("UsualClass.cs");
        CodeGenerator generator = new CodeGenerator();
        var dictionary = generator.GenerateTest(codeText);
        Assert.True(dictionary.ContainsKey("UsualClassTests"));
        Assert.IsNotNull(dictionary["UsualClassTests"]);
        FileScopedNamespaceDeclarationSyntax name = (FileScopedNamespaceDeclarationSyntax)dictionary["UsualClassTests"].GetRoot().ChildNodes().First();
        ClassDeclarationSyntax classNode = (ClassDeclarationSyntax)name.Members[0];
        
        Assert.AreEqual("TestsGenerator.Tests",name.Name.ToString());
        Assert.AreEqual("UsualClassTests",classNode.Identifier.ToString());
        Assert.AreEqual(3,classNode.Members.Count);
        Assert.AreEqual("OkTest",((MethodDeclarationSyntax)classNode.Members[0]).Identifier.Text);
        Assert.AreEqual("Test",((MethodDeclarationSyntax)classNode.Members[0]).AttributeLists[0].Attributes[0].Name.ToString());
        Assert.AreEqual("GiveMeI1Test",((MethodDeclarationSyntax)classNode.Members[1]).Identifier.Text);
        Assert.AreEqual("Test",((MethodDeclarationSyntax)classNode.Members[1]).AttributeLists[0].Attributes[0].Name.ToString());
        Assert.AreEqual("GiveMeI2Test",((MethodDeclarationSyntax)classNode.Members[2]).Identifier.Text);
        Assert.AreEqual("Test",((MethodDeclarationSyntax)classNode.Members[2]).AttributeLists[0].Attributes[0].Name.ToString());

    }
    
    [Test]
    public void SampleClassTesting()
    {
        string codeText = File.ReadAllText("SampleClass.cs");
        CodeGenerator generator = new CodeGenerator();
        var dictionary = generator.GenerateTest(codeText);
        Assert.True(dictionary.ContainsKey("SampleClassTests"));
        Assert.True(dictionary.ContainsKey("SampleClass1Tests"));
        Assert.IsNotNull(dictionary["SampleClassTests"]);
        Assert.IsNotNull(dictionary["SampleClass1Tests"]);
        FileScopedNamespaceDeclarationSyntax name1 = (FileScopedNamespaceDeclarationSyntax)dictionary["SampleClassTests"].GetRoot().ChildNodes().First();
        FileScopedNamespaceDeclarationSyntax name2 = (FileScopedNamespaceDeclarationSyntax)dictionary["SampleClass1Tests"].GetRoot().ChildNodes().First();
        ClassDeclarationSyntax classNode1 = (ClassDeclarationSyntax)name1.Members[0];
        ClassDeclarationSyntax classNode2 = (ClassDeclarationSyntax)name2.Members[0];
        
        Assert.AreEqual("TestsGenerator.Tests",name1.Name.ToString());
        Assert.AreEqual("SampleClassTests",classNode1.Identifier.ToString());
        Assert.AreEqual(3,classNode1.Members.Count);
        Assert.AreEqual("FirstMethod1Test",((MethodDeclarationSyntax)classNode1.Members[0]).Identifier.Text);
        Assert.AreEqual("Test",((MethodDeclarationSyntax)classNode1.Members[0]).AttributeLists[0].Attributes[0].Name.ToString());
        Assert.AreEqual("FirstMethod2Test",((MethodDeclarationSyntax)classNode1.Members[1]).Identifier.Text);
        Assert.AreEqual("Test",((MethodDeclarationSyntax)classNode1.Members[1]).AttributeLists[0].Attributes[0].Name.ToString());
        Assert.AreEqual("MethodForParseTest",((MethodDeclarationSyntax)classNode1.Members[2]).Identifier.Text);
        Assert.AreEqual("Test",((MethodDeclarationSyntax)classNode1.Members[2]).AttributeLists[0].Attributes[0].Name.ToString());

        Assert.AreEqual("TestsGenerator.Tests",name2.Name.ToString());
        Assert.AreEqual("SampleClass1Tests",classNode2.Identifier.ToString());
        Assert.AreEqual(2,classNode2.Members.Count);
        Assert.AreEqual("MainTest",((MethodDeclarationSyntax)classNode2.Members[0]).Identifier.Text);
        Assert.AreEqual("Test",((MethodDeclarationSyntax)classNode2.Members[0]).AttributeLists[0].Attributes[0].Name.ToString());
        Assert.AreEqual("MethodForParseTest",((MethodDeclarationSyntax)classNode2.Members[1]).Identifier.Text);
        Assert.AreEqual("Test",((MethodDeclarationSyntax)classNode2.Members[1]).AttributeLists[0].Attributes[0].Name.ToString());
        
    }
}