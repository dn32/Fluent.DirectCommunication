using PusherServer;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Fluent.DirectCommunicationPusher
{
    public class DuplexConnection<T> : IDisposable where T : new()
    {
        protected CancellationToken CancellationToken { get; set; }
        private Type ClientOperationType { get; set; }
        public ClientRXProcessor ClientRXProcessor { get; set; }
        public PusherServer.Pusher ConnectionToSend { get; set; }
        public PusherClient.Pusher ConnectionToReceive { get; set; }

        #region RX

        public DuplexConnection(string channel)
        {
            ClientOperationType = typeof(T);
            ClientRXProcessor = new ClientRXProcessor(ClientOperationType);

            CreateConnectionToReceive(channel);
            ConnectToSend();
        }

        public void CreateConnectionToReceive(string channel)
        {
            ConnectionToReceive = new PusherClient.Pusher("5569ed05c179202d39a4");
            ConnectionToReceive.ConnectionStateChanged += ConnectionStateChange;
            ConnectionToReceive.Error += ConnectionError;
            ConnectionToReceive.ConnectAsync();

            ConnectionToReceive.SubscribeAsync(channel).Result.BindAll((string method, dynamic data) =>
            {

                var return_ = ClientRXProcessor.ReceiveMessage(channel, method, data) as object;
                if(return_ != null)
                {
                    ConnectionToSend.Call(($"{channel}_RETURN"), "Return", return_);
                }

            });
        }

        private void ConnectionStateChange(object sender, PusherClient.ConnectionState state)
        {
            ("Connection state: " + state.ToString()).Message();
        }

        private void ConnectionError(object sender, PusherClient.PusherException error)
        {
            Util.Message("Pusher Channels Error: " + error.ToString());
        }

        public void Dispose()
        {
            ConnectionToReceive?.UnbindAll();
            ConnectionToReceive?.DisconnectAsync();
        }

        #endregion

        #region TX

        public void ConnectToSend()
        {
            var options = new PusherServer.PusherOptions { Cluster = "mt1", Encrypted = true };
            ConnectionToSend = new PusherServer.Pusher("819722", "5569ed05c179202d39a4", "0ea48de89d8aee835eea", options);
        }

        public string[] GetCannels()
        {
            var canaisAtivos = ConnectionToSend.FetchStateForChannelsAsync<ChannelsList>().Result;
            var channels = canaisAtivos.Data.Channels.Select(x => x.Key).ToArray();
            return channels;
        }

        #endregion

        public object CallAndResult(string channel, string method, object data, int timeOutMs = 10000)
        {
            object return_ = null;
            ConnectionToReceive.SubscribeAsync($"{channel}_RETURN").Result.BindAll((string method_, dynamic data_) =>
            {
                return_ = data_;
                ConnectionToReceive.Unbind($"{channel}_RETURN");
            });

            ConnectionToSend.Call(channel, method, data);

            var timer = new Stopwatch();
            timer.Start();

            while (true)
            {
                if (CancellationToken.IsCancellationRequested) { return null; }

                if (return_ == null)
                {
                    Thread.Sleep(5);
                }
                else
                {
                    return return_;
                }

                if (timer.ElapsedMilliseconds >= timeOutMs) { throw new TimeoutException($"Timeout invoking method {method}"); }
            }
        }
    }
}
