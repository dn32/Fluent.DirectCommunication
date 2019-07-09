using Fluent.DirectCommunicationPusher;
using Newtonsoft.Json;
using System;

namespace Fluent.DirectCommunication.Client
{
    public class TestClientOperations : ClientOperations
    {
        public object TestMethod(dynamic data)
        {
            var json = JsonConvert.SerializeObject(data);
            var client = JsonConvert.DeserializeObject<Client>(json) as Client;
            Console.WriteLine(json);
            return new Client { Name = "Client Return" };
        }
    }
}
