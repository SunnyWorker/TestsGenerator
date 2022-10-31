using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsGenerator;

public class Pipeline
{
    
    private volatile int _readingCount;
    private volatile int _processingCount;
    private volatile int _writingCount;
    
    private readonly PipelineConfiguration _pipelineConfiguration;
    
    public CodeWalker CodeWalker = new();

    public readonly string writingPath;
    public ConcurrentBag<int> NumberOfReadingTasks { get; }
    public ConcurrentBag<int> NumberOfProcessingTasks { get; }
    public ConcurrentBag<int> NumberOfWritingTasks { get; }
    
    public Pipeline(PipelineConfiguration pipelineConfiguration, string writingPath)
    {
        this.writingPath = writingPath;
        _pipelineConfiguration = pipelineConfiguration;
        NumberOfReadingTasks = new ConcurrentBag<int>();
        NumberOfProcessingTasks = new ConcurrentBag<int>();
        NumberOfWritingTasks = new ConcurrentBag<int>();
    }
    private void ChangeCodeWalker()
    {
        foreach (var key in CodeWalker.Methods.Keys)
        {
            for (int i = 0; i < CodeWalker.Methods[key].Count; i++)
            {
                if (CodeWalker.Methods[key].FindAll(s => s.Equals(CodeWalker.Methods[key][i])).Count > 1)
                {
                    string name = CodeWalker.Methods[key][i];
                    int j = 1;
                    for (int k = 0; k < CodeWalker.Methods[key].Count; k++)
                    {
                        if (CodeWalker.Methods[key][k].Equals(name))
                        {
                            CodeWalker.Methods[key][k] = name + j++;
                        }
                    }
                }
            }
        }
    }

    public async Task PerformProcessing(List<string> files)
    {
        _readingCount = 0;
        _processingCount = 0;
        _writingCount = 0;
        NumberOfProcessingTasks.Clear();
        NumberOfProcessingTasks.Clear();
        NumberOfWritingTasks.Clear();
        
        var linkOptions = new DataflowLinkOptions {PropagateCompletion = true};
        var readingBlock = new TransformBlock<string, string>(
            async path => await ReadFile(path),
            new ExecutionDataflowBlockOptions{MaxDegreeOfParallelism = _pipelineConfiguration.MaxReadingTasks});
        var processingBlock = new TransformBlock<string, Dictionary<string,SyntaxTree>>(
            content => ProcessFile(content),
            new ExecutionDataflowBlockOptions{MaxDegreeOfParallelism = _pipelineConfiguration.MaxProcessingTasks});
        var writingBlock = new ActionBlock<Dictionary<string,SyntaxTree>>(async fwc => await WriteFile(fwc),
            new ExecutionDataflowBlockOptions{MaxDegreeOfParallelism = _pipelineConfiguration.MaxWritingTasks});
        
        readingBlock.LinkTo(processingBlock, linkOptions);
        processingBlock.LinkTo(writingBlock, linkOptions);
        
        foreach (string file in files)
        {
            readingBlock.Post(file);
        }
        
        readingBlock.Complete();

        await writingBlock.Completion;
    }

    private async Task<string> ReadFile(string filePath)
    {
        int incremented = Interlocked.Increment(ref _readingCount);
        NumberOfReadingTasks.Add(incremented);
        string result;
        using (var streamReader = new StreamReader(filePath))
        {
            result = await streamReader.ReadToEndAsync();
        }
        
        Interlocked.Decrement(ref _readingCount);
        return result;
    }

    private Dictionary<string,SyntaxTree> ProcessFile(string content)
    {
        int incremented = Interlocked.Increment(ref _processingCount);
        NumberOfProcessingTasks.Add(incremented);
        var root = CSharpSyntaxTree.ParseText(content).GetRoot();
        Dictionary<string,SyntaxTree> SyntaxTrees = new();
        CodeWalker.Visit(root);
        ChangeCodeWalker();
        foreach (var classString in CodeWalker.Methods.Keys)
        {
            var methodList = new MemberDeclarationSyntax[CodeWalker.Methods[classString].Count];
            int j = 0;
            foreach (var childNodeString in CodeWalker.Methods[classString])
            {
                methodList[j++]=(MethodDeclaration
                    (PredefinedType
                        (Token
                            (SyntaxKind.VoidKeyword)),
                        Identifier(childNodeString + "Test"))
                    .WithAttributeLists(
                        SingletonList<AttributeListSyntax>(
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("Test"))))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword))).WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("Assert"),
                                                IdentifierName("Fail")))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList<ArgumentSyntax>(
                                                    Argument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal("autogenerated")))))))))));
            }
            SyntaxTrees.Add(classString + "Tests",SyntaxTree(CompilationUnit()
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        FileScopedNamespaceDeclaration(
                                IdentifierName(CodeWalker.fileNamespace))
                            .WithMembers(new SyntaxList<MemberDeclarationSyntax>((ClassDeclaration(classString + "Tests")
                                .WithAttributeLists(
                                    SingletonList<AttributeListSyntax>(
                                        AttributeList(
                                            SingletonSeparatedList<AttributeSyntax>(
                                                Attribute(
                                                    IdentifierName("TestFixture"))))))
                                .WithModifiers(TokenList
                                (Token
                                    (SyntaxKind.PublicKeyword)))
                                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(methodList))))
                            )))
                .NormalizeWhitespace()));
            
        }
        Interlocked.Decrement(ref _processingCount);
        return SyntaxTrees;
    }

    private async Task WriteFile(Dictionary<string,SyntaxTree> list)
    {
        foreach (var keyValuePair in list)
        {
            int incremented = Interlocked.Increment(ref _writingCount);
            NumberOfWritingTasks.Add(incremented);
            Console.WriteLine(writingPath+"\\"+keyValuePair.Key+".cs");
            using (var streamWriter = new StreamWriter(writingPath+"\\"+keyValuePair.Key+".cs"))
            {
                await streamWriter.WriteAsync(keyValuePair.Value.ToString());
            }
            Interlocked.Decrement(ref _writingCount);
        }
    }
}