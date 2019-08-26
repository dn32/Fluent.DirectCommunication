using Newtonsoft.Json;
using PusherServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Fluent.DirectCommunicationPusher
{
    //public class Authorizer : PusherClient.IAuthorizer
    //{
    //    private readonly string _userName;

    //    public Authorizer(string userName)
    //    {
    //        _userName = userName;
    //    }

    //    public string Authorize(string channelName, string socketId)
    //    {
    //        var Config = new Credentials
    //        {
    //            AppId = "819722",
    //            AppKey = "5569ed05c179202d39a4",
    //            AppSecret = "0ea48de89d8aee835eea",
    //            Options = new PusherServer.PusherOptions { Cluster = "mt1", Encrypted = true }
    //        };

    //        var provider = new PusherServer.Pusher(Config.AppId, Config.AppKey, Config.AppSecret);

    //        string authData;

    //        if (channelName.StartsWith("presence-"))
    //        {
    //            var channelData = new PresenceChannelData
    //            {
    //                user_id = socketId,
    //                user_info = new Info
    //                {
    //                    Name = _userName,
    //                    Teste = "123"
    //                }
    //            };

    //            authData = provider.Authenticate(channelName, socketId, channelData).ToJson();
    //        }
    //        else
    //        {
    //            authData = provider.Authenticate(channelName, socketId).ToJson();
    //        }

    //        return authData;
    //    }
    //}

    public partial class DuplexConnection : IDisposable
    {
        #region PROP

        private CancellationToken CancellationToken { get; set; }
        private PusherServer.Pusher ConnectionToSend { get; set; }
        private PusherClient.Pusher ConnectionToReceive { get; set; }
        private List<Type> Implements { get; set; }
        private Credentials Credentials { get; set; }
        public string ClientId { get; }
        public string Canal { get; }

        #endregion

        #region RX

        public DuplexConnection(string clientId, Credentials credentials)
        {
            ClientId = clientId;
            Credentials = credentials;
            ConnectToSend();

            new Thread(() => CreateConnectionToReceive(clientId)).Start();

            var shutdownCts = new CancellationTokenSource();
            CancellationToken = shutdownCts.Token;
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => shutdownCts.Cancel();
        }

        public void CreateConnectionToReceive(string receptionChannel)
        {
            inicio:

            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                ConnectionToReceive = new PusherClient.Pusher(Credentials.AppKey);
                ConnectionToReceive.ConnectionStateChanged += ConnectionStateChanged;
                ConnectionToReceive.Error += Erro;
                ConnectionToReceive.Disconnected += Disconnected;
                ConnectionToReceive.Connected += Connected;

                void ConnectionStateChanged(object sender, PusherClient.ConnectionState state)
                {
                    Util.Message("DuplexConnection state: " + state.ToString());
                }

                void Erro(object sender, PusherClient.PusherException error)
                {
                    Util.Message("DuplexConnection Error: " + error.ToString());
                }

                void Connected(object sender)
                {
                    Util.Message("DuplexConnection is Conected!");

                    ConnectionToReceive.SubscribeAsync(receptionChannel).Result.BindAll((string method, dynamic data) =>
                    {
                        if (method == ContractOfReturn.OPERATION_RETURN_NAME) { return; }

                        var transmissionContract = Util.DynamicToObject<TransmissionContract>(data) as TransmissionContract;
                        var return_ = Execute(transmissionContract);
                        TXExtension.ReturnToClient(ConnectionToSend, transmissionContract.ReturnIdChannel, return_);
                    });
                }

                void Disconnected(object sender)
                {
                    Util.Message("DuplexConnection is Disconnected.");
                    Unsubscribe(receptionChannel);
                    ConnectionToReceive?.UnbindAll();
                    Thread.Sleep(20000);
                    try { Dispose(); } catch (Exception) { }
                    CreateConnectionToReceive(receptionChannel);
                }

                Util.Message("DuplexConnection Starting...");
                ConnectionToReceive.ConnectAsync();
            }
            catch (Exception ex)
            {
                try { Dispose(); } catch (Exception) { }
                Console.WriteLine($"Exception: {ex.Message}");
                Thread.Sleep(20000);
                goto inicio;
            }
        }

        private void Unsubscribe(string channelName)
        {
            if (ConnectionToReceive.Channels.TryGetValue(channelName, out PusherClient.Channel channel))
            {
                channel.Unsubscribe();

                ConnectionToReceive.Channels.TryRemove(channelName, out _);
            }
        }

        private ContractOfReturn Execute(TransmissionContract data)
        {
            try
            {
                if (Implements == null) { Implements = Util.GetAllIRequestControllerImplements(); }

                var first = Implements.FirstOrDefault(x => x.Name.Equals(data.Operation, StringComparison.InvariantCultureIgnoreCase));

                if (first == null) { throw new Exception($"No Request Controller implementation found with name {data.Operation}"); }

                var firstInstance = Activator.CreateInstance(first) as IRequestController;

                return firstInstance.Invoke(data) as ContractOfReturn;
            }
            catch (Exception ex)
            {
                return new ContractOfReturn
                {
                    Sucess = false,
                    Ex = JsonConvert.SerializeObject(ex.Message)
                };
            }
        }

        public void Dispose()
        {
            ConnectionToReceive?.UnbindAll();
            if (ConnectionToReceive.State == PusherClient.ConnectionState.Connected)
            {
                ConnectionToReceive?.DisconnectAsync();
            }

            foreach (var channel in ConnectionToReceive.Channels)
            {
                channel.Value.Unsubscribe();
            }

            ConnectionToReceive = null;
        }

        #endregion

        #region TX

        public void ConnectToSend()
        {
            ConnectionToSend = new PusherServer.Pusher(Credentials.AppId, Credentials.AppKey, Credentials.AppSecret, Credentials.Options);
        }

        #endregion

        #region OPERATION

        public string[] GetClients()
        {
            try
            {
                var canaisAtivos = ConnectionToSend.GetAsync<ChannelsList>("/channels").Result;
                return canaisAtivos.Data.Channels.Select(x => x.Key).ToArray();
            }
            catch (Exception ex)
            {
                Util.Message($"DuplexConnection GetClients Error: {ex.Message}");
                return new string[] { };
            }
        }

        public string[] GetClientsByPrefix(string prefix)
        {
            var canaisAtivos = ConnectionToSend.GetAsync<ChannelsList>("/channels", new { filter_by_prefix = prefix }).Result;
            return canaisAtivos.Data.Channels.Select(x => x.Key).ToArray();
        }

        public void Call(TransmissionContract txData)
        {
            ConnectionToSend.ExecuteOnClient(txData);
        }

        public ContractOfReturn CallAndResult(TransmissionContract txData, int timeOutMs = 10000, string socketId = null)
        {
            try
            {
                ContractOfReturn return_ = null;
                txData.ReturnIdChannel = txData.Sender + "_RX_" + Guid.NewGuid().ToString();
                ConnectionToReceive.SubscribeAsync(txData.ReturnIdChannel).Result.BindAll((string method_, dynamic rxData) =>
                {
                    return_ = Util.DynamicToObject<ContractOfReturn>(rxData) as ContractOfReturn;
                    Unsubscribe(txData.ReturnIdChannel);
                });

                ConnectionToSend.ExecuteOnClient(txData, socketId);

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

                    if (timer.ElapsedMilliseconds >= timeOutMs)
                    {
                        ConnectionToReceive.Channels[txData.ReturnIdChannel].Unsubscribe();
                        throw new TimeoutException($"Time expired executing operation {txData.Operation} on client {txData.Destination}");
                    }
                }
            }
            catch (Exception ex)
            {
                return new ContractOfReturn
                {
                    Sucess = false,
                    Ex = JsonConvert.SerializeObject(ex.Message)
                };
            }
        }

        #endregion
    }
}
