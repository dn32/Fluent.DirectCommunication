using System;
using System.Threading;

namespace Fluent.DirectCommunication.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => { cts.Cancel(); };

            Console.WriteLine("Starting server....");
            new Connection<ClientOperations>("http://localhost:5000/FluentServerHub", $"CLIENTA", "GroupA", cancellationToken);
            Console.WriteLine("Server started successfully!");
            Console.ReadLine();
        }
    }
}
