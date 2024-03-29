﻿using Fluent.DirectCommunicationPusher;
using Newtonsoft.Json;
using System;

namespace Fluent.DirectCommunication.Client
{
    public class TestMethod : IRequestController
    {
        public ContractOfReturn Invoke(TransmissionContract data)
        {
            var client = JsonConvert.SerializeObject(data);
            Console.WriteLine($"RX: {client}");

            var ret = new LocalContractOfReturn()
            {
                DateTime = DateTime.Now
            };

            return ret;
        }
    }
}
