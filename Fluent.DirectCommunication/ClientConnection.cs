using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fluent.DirectCommunication
{
    public class Connection<T> where T : class, new()
    {
        private CancellationToken CancellationToken { get; }
        private HubConnection connection;
        private T ClientOperations { get; set; }
        private int MaxBufferSize { get; set; }
        private string AdditionalInformation { get; }
        private string Url { get; }
        private string Client { get; }
        private string Group { get; }
        private object Lock = new object();

        public Connection(string url, string client, string group, CancellationToken cancellationToken, int maxBufferSize = 10485760, string additionalInformation = "")
        {
            Url = url;
            Client = client;
            Group = group;
            CancellationToken = cancellationToken;
            MaxBufferSize = maxBufferSize;
            AdditionalInformation = additionalInformation;
        }

        public void Start()
        {
            ClientOperations = new T();
            connection = new HubConnectionBuilder()
                .WithUrl(Url)
                .Build();

            connection.Closed += async (error) =>
            {
                Connect(Client, Group, AdditionalInformation, Url);
            };

            Connect(Client, Group, AdditionalInformation, Url);
            StartEvent();
        }

        private void Connect(string client, string group, string additionalInformation, string url)
        {
            lock (Lock)
            {
            reconnect:
                try
                {
                    if (CancellationToken.IsCancellationRequested) { return; }
                    Message($"Conecting {url}...");

                    connection.StartAsync(CancellationToken).Wait();
                    connection.InvokeAsync("RegisterClient", client, group, additionalInformation, CancellationToken).Wait();
                    Message("Conected!");
                }
                catch (Exception)
                {
                    Task.Delay(5000 + new Random().Next(0, 5) * 1000, CancellationToken).Wait();
                    if (CancellationToken.IsCancellationRequested) { return; }
                    goto reconnect;
                }
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
                     if(internalMethod == null)
                     {
                         throw new Exception($"Method not found {method}");
                     }

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

        public async void InvokeAsync(string method, string client, string jsonParameters)
        {
            if (connection.State == HubConnectionState.Connected)
            {
                try
                {
                    await connection.InvokeAsync(method, client, jsonParameters, CancellationToken);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
