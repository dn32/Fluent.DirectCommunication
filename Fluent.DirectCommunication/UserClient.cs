using Microsoft.AspNetCore.SignalR;

namespace Fluent.DirectCommunication
{
    public class UserClient
    {
        public IClientProxy ClientProxy { get; }

        public string OperationExecutionId { get; set; }

        public string Client { get; }

        public string Group { get; }

        public object ReturnMethod { get; internal set; }

        public string AdditionalInformation { get; set; }

        public UserClient(IClientProxy clientProxy, string client, string group, string additionalInformation)
        {
            ClientProxy = clientProxy;
            Client = client;
            Group = group;
            AdditionalInformation = additionalInformation;
        }
    }
}
