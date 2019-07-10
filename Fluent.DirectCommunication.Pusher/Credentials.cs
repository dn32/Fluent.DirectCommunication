namespace Fluent.DirectCommunicationPusher
{
    public class Credentials
    {
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public PusherServer.IPusherOptions Options { get; set; }
    }
}
