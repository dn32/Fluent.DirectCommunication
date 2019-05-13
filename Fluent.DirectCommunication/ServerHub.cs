using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fluent.DirectCommunication
{
    public partial class ServerHub : Hub
    {
        public static Dictionary<string, UserClient> UserClients { get; set; } = new Dictionary<string, UserClient>();

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userClient = GetUser();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userClient.Group);
            await base.OnDisconnectedAsync(exception);
            UserClients.Remove(Context.ConnectionId);
        }

        private UserClient GetUser()
        {
            UserClients.TryGetValue(Context.ConnectionId, out UserClient clientUser);
            return clientUser;
        }

        public void Register(string client, string group)
        {
            UserClients.Add(Context.ConnectionId, new UserClient(Clients.Client(Context.ConnectionId), client, group));
            Groups.AddToGroupAsync(Context.ConnectionId, group).Wait();
            Console.WriteLine($"S - Register {client}");
        }

        public void ClientToServer(string client, object parameters)
        {
            Console.WriteLine($"S - ClientToServer {client} {parameters}");

            var client_ = Clients.Client(Context.ConnectionId);
            var connectionId = Context.ConnectionId;
            new Thread(() =>
            {
                InvokeClientMethodBefore(connectionId);
            }).Start();
        }

        public void Return(string client, object return_)
        {
            UserClients.TryGetValue(Context.ConnectionId, out UserClient userClient);
            userClient.ReturnMethod = return_;
        }

        private void InvokeClientMethodBefore(string connectionId)
        {
            UserClients.TryGetValue(connectionId, out UserClient userClient);
            var ret = userClient.Invoke("SelectSQL", new object[] { "select * from test" });
            Console.WriteLine($"select * from test: {ret}");
        }
    }
}
