//using System;
//using System.Diagnostics;
//using System.Threading;

//namespace Fluent.DirectCommunicationPusher
//{
//    public static class Extension
//    {
//        public static object Invoke(this UserClient userClient, string method, object data, int timeOutMs = 10000, bool wait = true)
//        {
//            var cts = new CancellationTokenSource();
//            var cancellationToken = cts.Token;
//            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => { cts.Cancel(); };


//            if (!string.IsNullOrWhiteSpace(userClient.OperationExecutionId))
//            {
//                throw new Exception($"An operation already running for {userClient.Client}");
//            }

//            userClient.OperationExecutionId = Guid.NewGuid().ToString();
//            userClient.ReturnMethod = null;
//            ServerConnection.ConnectToSend().CallMethodOnClient(userClient.Channel, method, data);

//            if (!wait)
//            {
//                userClient.ReturnMethod = null;
//                userClient.OperationExecutionId = "";
//                return null;
//            }

//            var t = new Stopwatch();
//            t.Start();

//            while (true)
//            {
//                if (t.ElapsedMilliseconds >= timeOutMs)
//                {
//                    t.Stop();
//                    userClient.OperationExecutionId = "";
//                    throw new TimeoutException($"Timeout invoking method {method}");
//                }

//                if (cancellationToken.IsCancellationRequested) { throw new TimeoutException($"Operation canceled"); }

//                Thread.Sleep(1);
//                if (userClient.ReturnMethod != null)
//                {
//                    userClient.OperationExecutionId = "";
//                    t.Stop();
//                    return userClient.ReturnMethod;
//                }
//            }
//        }
//    }
//}
