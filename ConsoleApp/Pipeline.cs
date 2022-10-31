using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using TestsGenerator;

namespace ConsoleApp;

public class Pipeline
{
    private volatile int _readingCount;
    private volatile int _processingCount;
    private volatile int _writingCount;
    
    private readonly PipelineConfiguration _pipelineConfiguration;

    private CodeGenerator codeGenerator;
    public readonly string writingPath;
    public ConcurrentBag<int> NumberOfReadingTasks { get; }
    public ConcurrentBag<int> NumberOfProcessingTasks { get; }
    public ConcurrentBag<int> NumberOfWritingTasks { get; }
    
    public Pipeline(PipelineConfiguration pipelineConfiguration, string writingPath)
    {
        this.writingPath = writingPath;
        this.codeGenerator = new();
        this._pipelineConfiguration = pipelineConfiguration;
        this.NumberOfReadingTasks = new ConcurrentBag<int>();
        this.NumberOfProcessingTasks = new ConcurrentBag<int>();
        this.NumberOfWritingTasks = new ConcurrentBag<int>();
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
        var result = codeGenerator.GenerateTest(content);
        Interlocked.Decrement(ref _processingCount);
        return result;
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