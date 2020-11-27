using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Server
{
    public class AsynchronousServer
    {
        //private static ConcurrentQueue<TcpClient> clientQueue = new ConcurrentQueue<TcpClient>();

        public static async Task Start()
        {
            //Task t1 = TaskLoopStart();
            //Task t2 = TCPListenStart();
            //await Task.WhenAll(new Task[] { t1, t2 });

            await TCPListenStart();
        }

        private static async Task CompleteTask(TcpClient client)
        {
            try
            {
                // Get a stream object for reading and writing
                using NetworkStream stream = client.GetStream();
                byte[] bytes = new byte[client.ReceiveBufferSize];

                // Loop to receive all the data sent by the client.
                int bytesSize;
                while ((bytesSize = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesSize);
                    Console.WriteLine($"[{DateTime.Now}] Processing: {data}");

                    string[] splitedData = data.Split('|');
                    if (splitedData.Length == 2)
                    {
                        int timeToSleep = Convert.ToInt32(splitedData[0]);
                        data = splitedData[1];
                        Console.WriteLine($"[{DateTime.Now}] Sleep for {timeToSleep} seconds as requested. Response message updated.");
                        await Task.Delay(timeToSleep * 1000);
                    }

                    // Process the data sent by the client.
                    data = data.ToUpper();

                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    await stream.WriteAsync(msg, 0, msg.Length);
                    Console.WriteLine($"[{DateTime.Now}] Sent: {data}");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                client.Close();
            }
        }

        //private static async Task TaskLoopStart()
        //{
        //    while (true)
        //    {
        //        if (!clientQueue.TryDequeue(out TcpClient client))
        //        {
        //            Console.WriteLine("No Task in Queue");
        //            await Task.Delay(100);
        //            continue;
        //        }

        //        await CompleteTask(client);
        //    }
        //}

        private static async Task TCPListenStart()
        {
            List<Task> taskPool = new List<Task>();

            TcpListener server = null;

            bool keepRunning = true;

            Console.CancelKeyPress += async delegate (object sender, ConsoleCancelEventArgs e) {
                keepRunning = false;
                e.Cancel = true;
            };

            try
            {
                // Set the TcpListener on port 13000.
                int port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (keepRunning)
                {
                    Console.Write($"[{DateTime.Now}] Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine($"[{DateTime.Now}] Connected! Request enqueued.");
                    //clientQueue.Enqueue(client);

                    taskPool.Add(CompleteTask(client));
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                Console.WriteLine("Waiting for all tasks to complete.");
                await Task.WhenAll(taskPool);
                // Stop listening for new clients.
                Console.WriteLine("Stoppping Server Listening.");
                server.Stop();
                Console.WriteLine("Server stopped");
            }
        }
    }

    public class TraditionalServer
    {
        public static void Start()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                int port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                byte[] bytes = new byte[256];
                string data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client =  server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    using NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        string[] splitedData = data.Split('|');
                        if (splitedData.Length == 2)
                        {
                            int timeToSleep = Convert.ToInt32(splitedData[0]);
                            data = splitedData[1];
                            Console.WriteLine($"Sleep for {timeToSleep} seconds as requested. Response message updated.");
                            Thread.Sleep(timeToSleep * 1000);
                        }

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            //TraditionalServer.Start();      // Blocking
            await AsynchronousServer.Start();
        }
    }
}
