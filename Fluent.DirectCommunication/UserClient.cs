using Microsoft.AspNetCore.SignalR;

namespace Fluent.DirectCommunication
{
    public partial class ServerHub
    {
        public class UserClient
        {
            public IClientProxy ClientProxy { get; }

            public string OperationExecutionId { get; set; }

            public string Client { get; }

            public string Group { get; }

            public object ReturnMethod { get; internal set; }

            public UserClient(IClientProxy clientProxy, string client, string group)
            {
                ClientProxy = clientProxy;
                Client = client;
                Group = group;
            }
        }
    }
}
