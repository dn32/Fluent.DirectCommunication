using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Fluent.DirectCommunication
{
    public class Connection<T> where T : class, new()
    {
        private HubConnection connection;

        private T ClientOperations { get; set; }

        public Connection(string url, string client, string group)
        {
            ClientOperations = new T();
            connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            Connect().Wait();
            StartEvent();
            connection.InvokeAsync("Register", client, group);
        }

        private async Task Connect()
        {
            await connection.StartAsync();
        }

        private void StartEvent()
        {
            connection.On<string, string, object[]>("ReceiveMessage", (method, operationExecutionId, parameters) =>
             {
                 var internalMethod = ClientOperations.GetType().GetMethod(method);
                 var ret = internalMethod.Invoke(ClientOperations, parameters);
                 if (internalMethod.ReturnType != typeof(void))
                 {
                     InvokeAsync("Return", operationExecutionId, ret);
                 }
             });
        }

        private async void InvokeAsync(string method, string client, object parameters)
        {
            await connection.InvokeAsync(method, client, parameters);
        }
    }
}
