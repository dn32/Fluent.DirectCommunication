using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fluent.DirectCommunication
{
    public class Connection<T> where T : class, new()
    {
        private HubConnection connection;

        private T ClientOperations { get; set; }
        public int MaxBufferSize { get; set; }

        public Connection(string url, string client, string group, int maxBufferSize = 10485760, string additionalInformation = "")
        {
            MaxBufferSize = maxBufferSize;
            ClientOperations = new T();
            connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            Connect(client, group, additionalInformation, url);
            StartEvent();
        }

        private void Connect(string client, string group, string additionalInformation, string url)
        {
        reconnect:
            try
            {
                Message($"Conecting {url}...");
                connection.StartAsync().Wait();
                connection.InvokeAsync("RegisterClient", client, group, additionalInformation).Wait();
                Message("Conected!");
            }
            catch (Exception)
            {
                Task.Delay(new Random().Next(0, 5) * 1000).Wait();
                goto reconnect;
            }
        }

        private void Message(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#endif
        }

        private void StartEvent()
        {
            connection.On<string, string, object[]>("ReceiveMessage", (method, operationExecutionId, parameters) =>
             {
                 var internalMethod = ClientOperations.GetType().GetMethod(method);

                 object ret = null;
                 var json = "";
                 try
                 {
                     ret = internalMethod.Invoke(ClientOperations, parameters);
                     json = JsonConvert.SerializeObject(ret);
                     if (json.Length >= MaxBufferSize)
                     {
                         throw new Exception("Return exceeds buffer size.");
                     }
                 }
                 catch (Exception ex)
                 {
                     ret = ex;
                     json = JsonConvert.SerializeObject(ex);
                 }

                 if (internalMethod.ReturnType != typeof(void))
                 {
                     InvokeAsync("Return", operationExecutionId, json);
                 }
             });
        }

        private async void InvokeAsync(string method, string client, object parameters)
        {
            if (connection.State == HubConnectionState.Connected)
            {
                try
                {
                    await connection.InvokeAsync(method, client, parameters);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
