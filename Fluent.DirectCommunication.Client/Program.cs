using Fluent.DirectCommunicationPusher;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace Fluent.DirectCommunication.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //var cts = new CancellationTokenSource();
            //var cancellationToken = cts.Token;
            //AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => { cts.Cancel(); };

            //Console.WriteLine("Starting server....");
            //new Connection<ClientOperations>("http://localhost:5000/FluentServerHub", $"CLIENTA", "GroupA", cancellationToken);
            //Console.WriteLine("Server started successfully!");


            var credentials = new Credentials
            {
                AppId = "819722",
                AppKey = "5569ed05c179202d39a4",
                AppSecret = "0ea48de89d8aee835eea",
                Options = new PusherServer.PusherOptions { Cluster = "mt1", Encrypted = true }
            };
            var clientId = "CLIENT-01";
            var conn = new DuplexConnection<TransmissionContract, LocalContractOfReturn>(clientId, credentials);

            {
                Thread.Sleep(2000);

                //Send to support
                var localTransmissionContract = new TransmissionContract
                {
                    Destination = "SUPPORT",
                    Operation = "SuportTest",
                    Sender = clientId
                };

                var ret = conn.CallAndResult(localTransmissionContract, int.MaxValue);
                var json = JsonConvert.SerializeObject(ret);
                Console.WriteLine($"Return: {json}");
            }

            Console.ReadLine();
        }
    }
}
