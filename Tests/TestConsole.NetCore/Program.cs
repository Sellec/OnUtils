using System;

namespace TestConsole.NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Data.EFCore.ProgramTest.Run();

            Console.WriteLine("Hello World!");

            Console.ReadKey();
        }

        static void TestRun()
        {
            Console.WriteLine($"{DateTime.Now}");
        }
    }
}
