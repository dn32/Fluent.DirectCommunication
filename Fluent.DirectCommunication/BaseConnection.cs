using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fluent.DirectCommunication
{
    public class BaseConnection : IDisposable
    {
        #region properties

        public static BaseConnection SharedConnection { get; set; }
        protected CancellationToken CancellationToken { get; set; }
        protected HubConnection LocalConnection { get; set; }
        protected int MaxBufferSize { get; set; }
        protected string AdditionalInformation { get; set; }
        protected string Url { get; set; }
        protected string Client { get; set; }
        protected string Group { get; set; }
        protected object Lock { get; set; } = new object();

        #endregion

        public BaseConnection()
        {
            SharedConnection = this;
        }

        protected void Connect(string client, string group, string additionalInformation, string url)
        {
            lock (Lock)
            {
            reconnect:
                try
                {
                    if (CancellationToken.IsCancellationRequested) { return; }
                    Message($"Conecting {url}...");

                    LocalConnection.StartAsync(CancellationToken).Wait();
                    LocalConnection.InvokeAsync("RegisterClient", client, group, additionalInformation, CancellationToken).Wait();
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

        public void Invoke(string method, string client, string jsonParameters, out Exception exception)
        {
            exception = null;
            if (LocalConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    LocalConnection.InvokeAsync(method, client, jsonParameters, CancellationToken);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
        }

        protected void Message(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#endif
        }

        public void Dispose()
        {
            LocalConnection.DisposeAsync();
        }
    }
}
