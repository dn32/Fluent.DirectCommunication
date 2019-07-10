using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Fluent.DirectCommunicationPusher
{
    public partial class DuplexConnection<TX, RX> : IDisposable where TX : TransmissionContract, new() where RX : ContractOfReturn, new()
    {
        #region PROP

        protected CancellationToken CancellationToken { get; set; }
        public PusherServer.Pusher ConnectionToSend { get; set; }
        public PusherClient.Pusher ConnectionToReceive { get; set; }
        public List<Type> Implements { get; set; }
        public Credentials Credentials { get; set; }

        #endregion

        #region RX

        public DuplexConnection(string clientId, Credentials credentials)
        {
            Credentials = credentials;
            CreateConnectionToReceive(clientId);
            ConnectToSend();
        }

        public void CreateConnectionToReceive(string receptionChannel)
        {
            ConnectionToReceive = new PusherClient.Pusher(Credentials.AppKey);
            ConnectionToReceive.ConnectionStateChanged += (object sender, PusherClient.ConnectionState state) => Util.Message("Connection state: " + state.ToString()); ;
            ConnectionToReceive.Error += (object sender, PusherClient.PusherException error) => Util.Message("Pusher Channels Error: " + error.ToString()); ;
            ConnectionToReceive.ConnectAsync();

            ConnectionToReceive.SubscribeAsync(receptionChannel).Result.BindAll((string method, dynamic data) =>
            {
                if (method == ContractOfReturn.OPERATION_RETURN_NAME) { return; }

                var transmissionContract = Util.DynamicToObject<TX>(data) as TX;
                var return_ = Execute(transmissionContract);
                TXExtension.ReturnToClient(ConnectionToSend, transmissionContract.ReturnIdChannel, return_);
            });
        }

        private RX Execute(TX data)
        {
            try
            {
                if (Implements == null) { Implements = Util.GetAllIRequestControllerImplements(); }

                var first = Implements.FirstOrDefault(x => x.Name.Equals(data.Operation, StringComparison.InvariantCultureIgnoreCase));

                if (first == null) { throw new Exception($"No Request Controller implementation found with name {data.Operation}"); }

                var firstInstance = Activator.CreateInstance(first) as IRequestController;

                var ret = firstInstance.Invoke(data) as RX;
                ret.Sucess = true;
                return ret;
            }
            catch (Exception ex)
            {
                return new RX
                {
                    Sucess = false,
                    Ex = JsonConvert.SerializeObject(ex.Message)
                };
            }
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
            //var options = new PusherServer.PusherOptions { Cluster = "mt1", Encrypted = true };
            //ConnectionToSend = new PusherServer.Pusher("819722", "5569ed05c179202d39a4", "0ea48de89d8aee835eea", options);
            ConnectionToSend = new PusherServer.Pusher(Credentials.AppId, Credentials.AppKey, Credentials.AppSecret, Credentials.Options);
        }

        public string[] GetClients()
        {
            var canaisAtivos = ConnectionToSend.GetAsync<PusherServer.ChannelsList>("/channels", new { filter_by_prefix = "CLIENT-" }).Result;
            var channels = canaisAtivos.Data.Channels.Select(x => x.Key).ToArray();
            return channels;
        }

        #endregion

        public RX CallAndResult(TX txData, int timeOutMs = 10000)
        {
            RX return_ = null;
            txData.ReturnIdChannel = Guid.NewGuid().ToString();
            ConnectionToReceive.SubscribeAsync(txData.ReturnIdChannel).Result.BindAll((string method_, dynamic rxData) =>
            {
                return_ = Util.DynamicToObject<RX>(rxData) as RX;
                ConnectionToReceive.Unbind(txData.ReturnIdChannel);
            });

            ConnectionToSend.ExecuteOnClient(txData);

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

                if (timer.ElapsedMilliseconds >= timeOutMs) { throw new TimeoutException($"Time expired executing operation {txData.Operation} on client {txData.Destination}"); }
            }
        }
    }
}
