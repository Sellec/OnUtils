using System;

namespace TestConsole.NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }

        static void TestRun()
        {
            Console.WriteLine($"{DateTime.Now}");
        }
    }
}
