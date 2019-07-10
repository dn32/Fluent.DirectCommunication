using Fluent.DirectCommunicationPusher;
using Newtonsoft.Json;
using System;

namespace Fluent.DirectCommunication.Server
{
    public class SuportTest : IRequestController
    {
        public ContractOfReturn Invoke(TransmissionContract data)
        {
            var client = JsonConvert.SerializeObject(data);
            Console.WriteLine($"RX support: {client}");

            var ret = new LocalContractOfReturn()
            {
                DateTime = DateTime.Now
            };

            return ret;
        }
    }
}
