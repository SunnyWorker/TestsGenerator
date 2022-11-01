using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using TestsGenerator;

namespace ConsoleApp;

public class Pipeline
{

    private readonly PipelineConfiguration _pipelineConfiguration;

    public readonly string writingPath;

    public Pipeline(PipelineConfiguration pipelineConfiguration, string writingPath)
    {
        this.writingPath = writingPath;
        this._pipelineConfiguration = pipelineConfiguration;
    }

    public async Task PerformProcessing(List<string> files)
    {
        
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

        string result;
        using (var streamReader = new StreamReader(filePath))
        {
            result = await streamReader.ReadToEndAsync();
        }
        
        return result;
    }

    private Dictionary<string,SyntaxTree> ProcessFile(string content)
    {

        var codeGenerator = new CodeGenerator();
        var result = codeGenerator.GenerateTest(content);
        return result;
    }

    private async Task WriteFile(Dictionary<string,SyntaxTree> list)
    {
        foreach (var keyValuePair in list)
        {
            using (var streamWriter = new StreamWriter(writingPath+"\\"+keyValuePair.Key+".cs"))
            {
                await streamWriter.WriteAsync(keyValuePair.Value.ToString());
            }
        }
    }
}