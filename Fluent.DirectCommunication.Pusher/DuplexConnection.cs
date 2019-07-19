using Newtonsoft.Json;
using PusherServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Fluent.DirectCommunicationPusher
{
    public class Authorizer : PusherClient.IAuthorizer
    {
        private readonly string _userName;

        public Authorizer(string userName)
        {
            _userName = userName;
        }

        public string Authorize(string channelName, string socketId)
        {
            var Config = new Credentials
            {
                AppId = "819722",
                AppKey = "5569ed05c179202d39a4",
                AppSecret = "0ea48de89d8aee835eea",
                Options = new PusherServer.PusherOptions { Cluster = "mt1", Encrypted = true }
            };

            var provider = new PusherServer.Pusher(Config.AppId, Config.AppKey, Config.AppSecret);

            string authData;

            if (channelName.StartsWith("presence-"))
            {
                var channelData = new PresenceChannelData
                {
                    user_id = socketId,
                    user_info = new Info
                    {
                        Name = _userName,
                        Teste = "123"
                    }
                };

                authData = provider.Authenticate(channelName, socketId, channelData).ToJson();
            }
            else
            {
                authData = provider.Authenticate(channelName, socketId).ToJson();
            }

            return authData;
        }
    }

    public class Info
    {
        public string Name { get; set; }
        public string Teste { get; set; }
    }

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
        public Authorizer Authorizer { get; set; }

        #endregion

        #region RX

        public DuplexConnection(string clientId, Credentials credentials)
        {
            ClientId = clientId;
            Credentials = credentials;
            CreateConnectionToReceive(clientId);
            ConnectToSend();
        }

        public void CreateConnectionToReceive(string receptionChannel)
        {
            Authorizer = new Authorizer(receptionChannel);
            ConnectionToReceive = new PusherClient.Pusher(Credentials.AppKey);
            //ConnectionToReceive = new PusherClient.Pusher(Credentials.AppKey, new PusherClient.PusherOptions { Authorizer = Authorizer });
            ConnectionToReceive.ConnectionStateChanged += (object sender, PusherClient.ConnectionState state) => Util.Message("DuplexConnection state: " + state.ToString()); ;
            ConnectionToReceive.Error += (object sender, PusherClient.PusherException error) => Util.Message("DuplexConnection Client Error: " + error.ToString()); ;
            ConnectionToReceive.ConnectAsync();

            ConnectionToReceive.SubscribeAsync(receptionChannel).Result.BindAll((string method, dynamic data) =>
             {
                 if (method == ContractOfReturn.OPERATION_RETURN_NAME) { return; }

                 var transmissionContract = Util.DynamicToObject<TransmissionContract>(data) as TransmissionContract;
                 var return_ = Execute(transmissionContract);
                 TXExtension.ReturnToClient(ConnectionToSend, transmissionContract.ReturnIdChannel, return_);
             });
        }

        private ContractOfReturn Execute(TransmissionContract data)
        {
            try
            {
                if (Implements == null) { Implements = Util.GetAllIRequestControllerImplements(); }

                var first = Implements.FirstOrDefault(x => x.Name.Equals(data.Operation, StringComparison.InvariantCultureIgnoreCase));

                if (first == null) { throw new Exception($"No Request Controller implementation found with name {data.Operation}"); }

                var firstInstance = Activator.CreateInstance(first) as IRequestController;

                var ret = firstInstance.Invoke(data) as ContractOfReturn;
                ret.Sucess = true;
                return ret;
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
            ConnectionToReceive?.DisconnectAsync();
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
            //var canaisAtivos = ConnectionToSend.GetAsync<PusherServer.ChannelsList>("/channels", new { filter_by_prefix = "CLIENT-" }).Result;
            var canaisAtivos = ConnectionToSend.GetAsync<PusherServer.ChannelsList>("/channels").Result;
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
                    ConnectionToReceive.Unbind(txData.ReturnIdChannel);
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
                        ConnectionToReceive.Unbind(txData.ReturnIdChannel);
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
