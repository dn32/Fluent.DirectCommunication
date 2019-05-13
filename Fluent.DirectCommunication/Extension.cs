using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using static Fluent.DirectCommunication.ServerHub;

namespace Fluent.DirectCommunication
{
    public static class Extension
    {
        public static object Invoke(this UserClient userClient, string method, object[] parameter)
        {
            lock (userClient)
            {
                if (!string.IsNullOrWhiteSpace(userClient.OperationExecutionId))
                {
                    throw new Exception($"An operation already running for {userClient.Client}");
                }

                userClient.OperationExecutionId = Guid.NewGuid().ToString();
                userClient.ReturnMethod = null;
                userClient.ClientProxy.SendAsync("ReceiveMessage", method, userClient.OperationExecutionId, parameter).Wait();
            }

            while (true)
            {
                Thread.Sleep(3);
                if (userClient.ReturnMethod != null)
                {
                    userClient.OperationExecutionId = "";
                    return userClient.ReturnMethod;
                }
            }
        }
    }
}
