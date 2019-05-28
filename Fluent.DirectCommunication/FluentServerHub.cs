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
            if(userClient == null)
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userClient.Group);
            await base.OnDisconnectedAsync(exception);
            Message($"S - Unregister {userClient.Client}");
            UserClients.Remove(Context.ConnectionId);
            Disconnected(userClient);
        }

        private UserClient GetUser()
        {
            UserClients.TryGetValue(Context.ConnectionId, out UserClient clientUser);
            return clientUser;
        }

        public void Register(string client, string group)
        {
            RegisterClient(client, group, "");
        }

        public virtual void Disconnected(UserClient userClient) { }

        public virtual void Registered(UserClient userClient) { }

        public void RegisterClient(string client, string group, string additionalInformation)
        {
            UserClients.TryGetValue(Context.ConnectionId, out UserClient userClient);
            if (userClient == null)
            {
                userClient = new UserClient(Clients.Client(Context.ConnectionId), client, group, additionalInformation);
                UserClients.Add(Context.ConnectionId, userClient);
            }

            Groups.AddToGroupAsync(Context.ConnectionId, group).Wait();
            Message($"S - Register {client}");

            Registered(userClient);
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
