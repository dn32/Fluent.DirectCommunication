using Newtonsoft.Json;
using System;

namespace Fluent.DirectCommunicationPusher
{
    public static class TXExtension
    {
        public static PusherServer.ITriggerResult ExecuteOnClient(this PusherServer.Pusher pusher, TransmissionContract data, string socketId = null)
        {
            if (data.Data != null && data.Data.Length > 8 * 1024)
            {
                throw new Exception("Os dados a enviar ultrapassam o limite de 8KB, tendo o total de " + (data.Data.Length / 1024) + "KB");
            }

            return pusher.TriggerAsync(data.Destination, data.Operation, data, new PusherServer.TriggerOptions { SocketId = socketId }).Result;
        }

        public static PusherServer.ITriggerResult ReturnToClient(this PusherServer.Pusher pusher, string returnIdChannel, ContractOfReturn data, string socketId = null)
        {
            if (data.Data != null && data.Data.Length > 8 * 1024)
            {
                data.Ex = JsonConvert.SerializeObject(new Exception("Os dados a enviar ultrapassam o limite de 8KB, tendo o total de " + (data.Data.Length / 1024) + "KB"));
                data.Data = null;
                data.Sucess = false;
                Console.WriteLine(data.Ex);
            }

            if (socketId == null)
            {
                return pusher.TriggerAsync(returnIdChannel, ContractOfReturn.OPERATION_RETURN_NAME, data).Result;
            }
            else
            {
                return pusher.TriggerAsync(returnIdChannel, ContractOfReturn.OPERATION_RETURN_NAME, data, new PusherServer.TriggerOptions { SocketId = socketId }).Result;
            }
        }
    }
}
