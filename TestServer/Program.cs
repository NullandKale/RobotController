using System;
using RobotController.netcode;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            tcpServer t = new tcpServer();

            t.listen();
        }
    }
}
