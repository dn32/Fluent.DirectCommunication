//using PusherServer;
//using System;
//using System.Linq;
//using System.Threading;

//namespace Fluent.DirectCommunicationPusher
//{
//    public class ServerConnection : IDisposable
//    {
//        private Pusher Pusher { get; set; }

//        private ServerConnection() { }

//        public string[] GetCannels()
//        {
//            var canaisAtivos = Pusher.FetchStateForChannelsAsync<ChannelsList>().Result;
//            var channels = canaisAtivos.Data.Channels.Select(x => x.Key).ToArray();
//            return channels;
//        }

//        public ITriggerResult CallMethodOnClient(string channel, string method, object data)
//        {
//            return Pusher.TriggerAsync(new[] { channel }, method, data).Result;
//        }

//        public ITriggerResult CallMethodOnClient(string[] channels, string method, object data)
//        {
//            return Pusher.TriggerAsync(channels, method, data).Result;
//        }

//        public ITriggerResult ReturnCallMethodOnClient(string method, object data)
//        {
//            return Pusher.TriggerAsync("SERVER", method, data).Result;
//        }

//        public static ServerConnection ConnectToSend()
//        {
//            var instance = new ServerConnection();
//            var options = new PusherOptions { Cluster = "mt1", Encrypted = true };
//            instance.Pusher = new Pusher("819722", "5569ed05c179202d39a4", "0ea48de89d8aee835eea", options);
//            return instance;
//        }

//        //public static ServerConnection CallAndResult(string channel, string method, object data)
//        //{
//        //    ConnectToSend().CallMethodOnClient(channel, method, data);

//        //    return instance;
//        //}

//        public void Dispose()
//        {
//        }
//    }
//}
