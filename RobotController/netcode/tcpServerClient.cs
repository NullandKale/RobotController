using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RobotController.netcode
{
    public class tcpServerClient
    {
        byte[] data = new byte[1024 * 8];
        public Socket socket;
        public bool connected = false;
        public bool stop = false;

        public ConcurrentQueue<string> writeCache;
        public Dictionary<string, Action<string>> taggedReceivers;
        public List<Action<string>> receivers;

        public Thread readThread;
        public Thread writeThread;
        public tcpServerClient(Socket socket)
        {
            this.socket = socket;

            writeCache = new ConcurrentQueue<string>();
            taggedReceivers = new Dictionary<string, Action<string>>();
            receivers = new List<Action<string>>();

            Console.WriteLine("Socket connected to {0}", socket.RemoteEndPoint.ToString());
            connected = true;

            writeThread = new Thread(writeThreadMain);
            readThread = new Thread(readThreadMain);

            writeThread.IsBackground = true;
            writeThread.Start();
            readThread.IsBackground = true;
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

        public void writeline(string line)
        {
            byte[] msg = Encoding.ASCII.GetBytes(line + "\n");

            try
            {
                int bytesSent = socket.Send(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }

        public string readline()
        {
            string line = "";

            while (true)
            {
                int rec = socket.Receive(data);
                line += Encoding.ASCII.GetString(data, 0, rec);
                if (line.Contains("\n"))
                {
                    return line;
                }
            }
        }

        public void writeThreadMain()
        {
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
                            writeline(s);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Socket Error, " + e.Message);
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        public void readThreadMain()
        {
            while (!stop)
            {
                while (!connected)
                {
                    Thread.Sleep(1);
                }

                string line = "";

                try
                {
                    line = readline();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Socket Error, " + e.Message);
                }

                if (line.Length > 0)
                {
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

                Thread.Sleep(1);
            }
        }
    }
}
