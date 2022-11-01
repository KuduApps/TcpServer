using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;

namespace TcpServer
{
    internal class Program
    {
        static void  Main(string[] args)
        {
            try
            {
                if (!int.TryParse(System.Environment.GetEnvironmentVariable("HTTP_PLATFORM_PORT"), out var port))
                {
                    port = int.Parse(args[0]);
                }

                var server = new TcpListener(IPAddress.Loopback, port);
                server.Start();
                WriteLine($"Listen on {port}");
                while (true)
                {
                    var result = server.BeginAcceptTcpClient(
                         new AsyncCallback(DoAcceptTcpClientCallback),
                         server);
                    result.AsyncWaitHandle.WaitOne();
                }

                server.Stop();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        public static void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            try
            {
                var listener = (TcpListener)ar.AsyncState;
                WriteLine("Client connected");
                using (var client = listener.EndAcceptTcpClient(ar))
                using (var ns = client.GetStream())
                using (var reader = new StreamReader(ns))
                {
                    var requestLine = reader.ReadLine();
                    WriteLine($"Request: {requestLine}");

                    var responseFile = Path.Combine(@"c:\home\site\wwwroot", string.Join("\\", requestLine.Split(' ')[1].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)));
                    if (!File.Exists(responseFile))
                    {
                        responseFile = @"c:\home\site\wwwroot\NotFound.txt";
                    }

                    var response = File.ReadAllBytes(responseFile);
                    ns.Write(response, 0, response.Length);
                    ns.Flush();
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        public static void WriteLine(object message)
        {
            try
            {
                Console.WriteLine($"{DateTime.UtcNow:s} {message}");
                File.AppendAllLines(@"c:\home\site\wwwroot\TcpServer.log", new[] { $"{DateTime.UtcNow:s} {message}" });
            }
            catch
            {

            }
        }
    }
}
