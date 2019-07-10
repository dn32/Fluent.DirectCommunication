using Fluent.DirectCommunicationPusher;
using System;

namespace Fluent.DirectCommunication.Server
{
    public class LocalContractOfReturn: ContractOfReturn
    {
        public LocalContractOfReturn() { }

        public DateTime DateTime { get; set; }
    }
}
