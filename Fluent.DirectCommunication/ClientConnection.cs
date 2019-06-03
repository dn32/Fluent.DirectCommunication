using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fluent.DirectCommunication
{
    public class Connection<T> : BaseConnection where T : new()
    {
        private T ClientOperations { get; set; }

        public Connection(string url, string client, string group, CancellationToken cancellationToken, int maxBufferSize = 10485760, string additionalInformation = "")
        {
            Url = url;
            Client = client;
            Group = group;
            CancellationToken = cancellationToken;
            MaxBufferSize = maxBufferSize;
            AdditionalInformation = additionalInformation;
        }

        protected void StartEvent()
        {
            LocalConnection.On<string, string, object[]>("ReceiveMessage", (method, operationExecutionId, parameters) =>
            {
                try
                {
                    new Thread(() => ReceiveMessage(method, operationExecutionId, parameters)).Start();
                }
                catch (Exception ex)
                {
                    Message($"Exception on ReceiveMessage {ex.Message}");
                }
            });
        }

        private void ReceiveMessage(string method, string operationExecutionId, object[] parameters)
        {
            object ret = null;
            var json = "";
            MethodInfo internalMethod = null;

            try
            {
                internalMethod = ClientOperations.GetType().GetMethod(method);
            }
            catch (Exception ex)
            {
                ret = ex;
                json = JsonConvert.SerializeObject(ex);
            }

            if (ret == null)
            {
                try
                {
                    if (internalMethod == null)
                    {
                        throw new Exception($"Method not found {method}");
                    }

                    ret = internalMethod.Invoke(ClientOperations, parameters);
                    if (ret != null)
                    {
                        json = JsonConvert.SerializeObject(ret);
                        if (json.Length >= MaxBufferSize)
                        {
                            throw new Exception("Return exceeds buffer size.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ret = ex;
                    json = JsonConvert.SerializeObject(ex);
                }
            }

            if (internalMethod?.ReturnType != typeof(void))
            {
                Invoke("Return", operationExecutionId, json, out Exception ex);
                ret = ex;
                json = JsonConvert.SerializeObject(ex);
            }
        }

        public void Start()
        {
            ClientOperations = new T();
            LocalConnection = new HubConnectionBuilder()
                .WithUrl(Url)
                .Build();

            LocalConnection.Closed += async (error) =>
            {
                if (CancellationToken.IsCancellationRequested) { return; }

                await Task.Run(() =>
                {
                    Connect(Client, Group, AdditionalInformation, Url);
                });
            };

            Connect(Client, Group, AdditionalInformation, Url);
            StartEvent();
        }
    }
}
