using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RobotController.netcode
{
    public class tcpClient
    {
        public string ip;
        public int port = 50807;
        public Socket socket;

        public bool connected = false;
        public bool stop = false;

        public ConcurrentQueue<string> writeCache;
        public Dictionary<string, Action<string>> taggedReceivers;
        public List<Action<string>> receivers;

        public Thread readThread;
        public Thread writeThread;

        public tcpClient(string ip)
        {
            this.ip = ip;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            writeCache = new ConcurrentQueue<string>();
            taggedReceivers = new Dictionary<string, Action<string>>();
            receivers = new List<Action<string>>();
        }

        public void connect()
        {
            try
            {
                socket.Connect(ip, port);
                Console.WriteLine("Socket connected to {0}", socket.RemoteEndPoint.ToString());
                connected = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
                return;
            }

            writeThread = new Thread(writeThreadMain);
            readThread = new Thread(readThreadMain);

            writeThread.Start();
            readThread.Start();
        }

        public void close()
        {
            stop = true;
            connected = false;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            writeThread.Join();
            readThread.Join();
        }

        public void writeline(NetworkStream stream, string line)
        {
            byte[] msg = Encoding.ASCII.GetBytes(line + "\n");
            stream.Write(msg, 0, msg.Length);
        }

        public void writeThreadMain()
        {
            NetworkStream networkStream = new NetworkStream(socket);

            while (!stop)
            {
                while (!connected)
                {
                    Thread.Sleep(1);
                }

                if (writeCache.Count > 0)
                {
                    while (writeCache.TryDequeue(out string s))
                    {
                        try
                        {
                            writeline(networkStream, s);
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine("Socket Error, " + e.Message);
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        public void readThreadMain()
        {
            StreamReader s = new StreamReader(new NetworkStream(socket));

            while (!stop)
            {
                while (!connected)
                {
                    Thread.Sleep(1);
                }


                string line = s.ReadLine();

                if(line != null)
                {
                    if (line.Length > 0)
                    {
                        int index = line.IndexOf("\n");
                        if (index > 0)
                        {
                            line = line.Substring(0, index);
                        }
                        if (line.Contains("|"))
                        {
                            string[] split = line.Split("|");
                            if (taggedReceivers.ContainsKey(split[0]))
                            {
                                taggedReceivers[split[0]].Invoke(split[1]);
                            }
                        }

                        for (int i = 0; i < receivers.Count; i++)
                        {
                            receivers[i].Invoke(line);
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }
    }
}
