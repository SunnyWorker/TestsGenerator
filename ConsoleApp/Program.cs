// See https://aka.ms/new-console-template for more information

using ConsoleApp;

public class Program
{
    public async static Task Main()
    {
        string fileListPath;
        string testFilesPath = null;
        int maxReadingTasks;
        int maxProcessingTasks;
        int maxWritingTasks;        
        List<string> files = new();
        bool success = false;
        while (!success)
        {
            try
            {
                Console.WriteLine("Введите путь к файлу со списком исходных файлов для генерации тестов:");
                fileListPath = Console.ReadLine();
                TextReader textReader = File.OpenText(fileListPath);
                string s;
                while ((s = textReader.ReadLine())!=null)
                {
                    files.Add(s);
                }

                success = true;
            }
            catch (IOException e)
            {
                success = false;
                Console.WriteLine("Введённые данные некорректны. Повторите попытку!");
            }
            catch (UnauthorizedAccessException e)
            {
                success = false;
                Console.WriteLine("Введённые данные некорректны. Повторите попытку!");
            }
            
        }

        success = false;
        while (!success)
        {
            Console.WriteLine("Введите путь к директории, где будут храниться тестовые файлы:");
            testFilesPath = Console.ReadLine();
            success = Directory.Exists(testFilesPath);
            if(!success) Console.WriteLine("Директории не существует. Повторите попытку!"); 
        }

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
