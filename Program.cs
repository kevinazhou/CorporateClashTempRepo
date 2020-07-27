using System;

namespace CCServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Corporate Clash Server";

            Server.Start(50, 26950);

            Console.ReadKey();
        }
    }
}
