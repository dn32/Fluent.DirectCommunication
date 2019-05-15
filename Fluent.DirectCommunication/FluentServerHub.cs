using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fluent.DirectCommunication
{
    public class FluentServerHub : Hub
    {
        public static Dictionary<string, UserClient> UserClients { get; set; } = new Dictionary<string, UserClient>();

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userClient = GetUser();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userClient.Group);
            await base.OnDisconnectedAsync(exception);
            Message($"S - Unregister {userClient.Client}");
            UserClients.Remove(Context.ConnectionId);
        }

        private UserClient GetUser()
        {
            UserClients.TryGetValue(Context.ConnectionId, out UserClient clientUser);
            return clientUser;
        }

        public virtual void Register(string client, string group)
        {
            if (!UserClients.ContainsKey(Context.ConnectionId))
            {
                UserClients.Add(Context.ConnectionId, new UserClient(Clients.Client(Context.ConnectionId), client, group));
            }

            Groups.AddToGroupAsync(Context.ConnectionId, group).Wait();
            Message($"S - Register {client}");
        }

        public void ClientToServer(string client, object parameters)
        {
            Message($"S - ClientToServer {client} {parameters}");

            var client_ = Clients.Client(Context.ConnectionId);
            var connectionId = Context.ConnectionId;
        }

        public void Return(string client, object return_)
        {
            UserClients.TryGetValue(Context.ConnectionId, out UserClient userClient);
            userClient.ReturnMethod = return_;
        }

        private void Message(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#endif
        }

    }
}
