using BasicWebServer;
using System;

namespace WebServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            server.Start();
            Console.ReadLine();
        }
    }
}
