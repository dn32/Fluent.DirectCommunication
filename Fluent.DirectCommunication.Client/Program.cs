using System;

namespace Fluent.DirectCommunication.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server....");
            new Connection<ClientOperations>("http://localhost:5000/FluentServerHub", $"CLIENTA", "GroupA");
            Console.WriteLine("Server started successfully!");
            Console.ReadLine();
        }
    }
}
