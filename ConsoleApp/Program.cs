// See https://aka.ms/new-console-template for more information

using ConsoleApp;
using TestsGenerator;

public class Program
{
    public async static Task Main()
    {
        string fileListPath;
        string testFilesPath;
        int maxReadingTasks;
        int maxProcessingTasks;
        int maxWritingTasks;
        Console.WriteLine("Введите путь к файлу со списком исходных файлов для генерации тестов:");
        List<string> files = new();
        fileListPath = Console.ReadLine();
        TextReader textReader = File.OpenText(fileListPath);
        string s;
        while ((s = textReader.ReadLine())!=null)
        {
            files.Add(s);
            s = textReader.ReadLine();
        }
        
        Console.WriteLine("Введите путь к директории, где будут храниться тестовые файлы:");
        testFilesPath = Console.ReadLine();
        
        Console.WriteLine("Введите максимальное количество файлов, загружаемых за раз:"); 
        maxReadingTasks = Convert.ToInt32(Console.ReadLine());
        
        Console.WriteLine("Введите максимальное количество одновременно обрабатываемых задач:"); 
        maxProcessingTasks = Convert.ToInt32(Console.ReadLine());
        
        Console.WriteLine("Введите максимальное количество одновременно записываемых файлов:"); 
        maxWritingTasks = Convert.ToInt32(Console.ReadLine());
        
        var pipeline = 
            new Pipeline(new PipelineConfiguration(maxReadingTasks,maxProcessingTasks,maxWritingTasks), testFilesPath);
        await pipeline.PerformProcessing(files);
    }
}

//D:\Unik\СПП\TestsGenerator\testList.txt
//D:\Unik\СПП\TestsGenerator\outputTests
