namespace Fluent.DirectCommunicationPusher
{
    public static class TXExtension
    {
        public static PusherServer.ITriggerResult ExecuteOnClient(this PusherServer.Pusher pusher, TransmissionContract data)
        {
            return pusher.TriggerAsync(data.Destination, data.Operation, data).Result;
        }

        public static PusherServer.ITriggerResult ReturnToClient(this PusherServer.Pusher pusher, string returnIdChannel, ContractOfReturn data)
        {
            return pusher.TriggerAsync(returnIdChannel, ContractOfReturn.OPERATION_RETURN_NAME, data).Result;
        }
    }
}
