namespace Fluent.DirectCommunicationPusher
{
    public static class TXExtension
    {
        public static PusherServer.ITriggerResult Call(this PusherServer.Pusher pusher, string channel, string method, object data)
        {
            return pusher.TriggerAsync(new[] { channel }, method, data).Result;
        }
    }
}
