namespace TestsGenerator
{
    public class SampleClass
    {
        public static void Main()
        {
            Console.WriteLine("Hello, World");
        }
        
        public static void Main(string s)
        {
            Console.WriteLine("Hello, World");
        }

        [Obsolete]
        public static void MethodForParse()
        {
            Console.WriteLine("I am ready, boy");
        }

        private int Method()
        {
            return 2;
        }
    }

    public class SampleClass1
    {
        public static void Main()
        {
            Console.WriteLine("Hello, World");
        }

        [Obsolete]
        public static void MethodForParse()
        {
            Console.WriteLine("I am ready, boy");
        }
    }
}