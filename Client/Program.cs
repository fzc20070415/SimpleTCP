using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        const int PORT_NO = 13000;
        const string SERVER_IP = "127.0.0.1";

        static async Task SendMessageThroughTCP(int taskID, string msg)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now}] {taskID}: Task initialized.");
                using TcpClient client = new TcpClient();
                byte[] bytesToSend = Encoding.ASCII.GetBytes(msg);

                // Asynchronsly attempt to connect to server
                await client.ConnectAsync(SERVER_IP, PORT_NO);
                Console.WriteLine($"[{DateTime.Now}] {taskID}: Connected to server.");

                // Write a message over the TCP Connection
                using NetworkStream nwStream = client.GetStream();
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                Console.WriteLine($"[{DateTime.Now}] {taskID}: Message Sent: {msg}");

                // Read server response
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                string msgReceived = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                Console.WriteLine($"[{DateTime.Now}] {taskID}: Message Received: {msgReceived}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{taskID} throws exception: {ex.Message}");
            }
        }

        static async Task Main(string[] args)
        {
            for (int x = 10; x > 0; x--)
            {
                Console.WriteLine($"Starting tasks in {x} seconds");
                Thread.Sleep(1000);
            }

            Task[] taskList = new Task[] {
                SendMessageThroughTCP(1, "5|Message 1 sent"),
                SendMessageThroughTCP(2, "6|Message 2 sent"),
                SendMessageThroughTCP(3, "1|Message 3 sent"),
                SendMessageThroughTCP(4, "5|Message 4 sent"),
                SendMessageThroughTCP(5, "3|Message 5 sent"),
                SendMessageThroughTCP(6, "0|Message 6 sent"),
            };
            await Task.WhenAll(taskList);
        }
    }
}
