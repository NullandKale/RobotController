using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RobotController.netcode
{
    public class tcpServer
    {
        public int port = 50807;
        public Socket listener;
        
        List<tcpServerClient> clients;
        Thread listenThread;

        public tcpServer()
        {
            clients = new List<tcpServerClient>();
            listenThread = new Thread(listen);
        }

        public void start()
        {
            listenThread.IsBackground = true;
            listenThread.Start();
        }


        public void writeLine(string message)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].writeCache.Enqueue(message);
            }
        }

        public void listen()
        {
            IPAddress[] ipv4Addresses = Array.FindAll(
            Dns.GetHostEntry(string.Empty).AddressList,
            a => a.AddressFamily == AddressFamily.InterNetwork);

            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(ipv4Addresses[0], port));
            listener.Listen(10);

            while(true)
            {
                Console.WriteLine("Waiting for a connection on " + listener.LocalEndPoint.ToString());
                Socket handler = listener.Accept();
                clients.Add(new tcpServerClient(handler));
            }
        }
    }
}
