using Microsoft.AspNetCore.SignalR;
using System;
using System.Diagnostics;
using System.Threading;

namespace Fluent.DirectCommunication
{
    public static class Extension
    {
        public static object Invoke(this UserClient userClient, string method, object[] parameter, CancellationToken CancellationToken, int timeOutMs = 10000)
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

            var t = new Stopwatch();
            t.Start();

            while (true)
            {
                if(t.ElapsedMilliseconds >= timeOutMs)
                {
                    t.Stop();
                    userClient.OperationExecutionId = "";
                    throw new TimeoutException($"Timeout invoking method {method}");
                }

                if (CancellationToken.IsCancellationRequested) { throw new TimeoutException($"Operation canceled"); }

                Thread.Sleep(1);
                if (userClient.ReturnMethod != null)
                {
                    userClient.OperationExecutionId = "";
                    t.Stop();
                    return userClient.ReturnMethod;
                }
            }
        }
    }
}
