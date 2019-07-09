using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Fluent.DirectCommunicationPusher
{
    public class ClientRXProcessor
    {
        public ClientRXProcessor(Type clientOperationType)
        {
            ClientOperationType = clientOperationType;
        }

        public Type ClientOperationType { get; }

        internal object ReceiveMessage(string channel, string method, dynamic data)
        {
            object ret = null;
            var json = "";
            MethodInfo internalMethod = null;

            try
            {
                internalMethod = ClientOperationType.GetMethod(method);
            }
            catch (Exception ex)
            {
                ret = ex;
                json = JsonConvert.SerializeObject(ex);
            }

            if (ret == null)
            {
                try
                {
                    if (internalMethod == null)
                    {
                        throw new Exception($"Method not found {method}");
                    }

                    var op = Activator.CreateInstance(ClientOperationType);
                    ret = internalMethod.Invoke(op, new object[] { data });
                    if (ret != null)
                    {
                        json = JsonConvert.SerializeObject(ret);
                        if (json.Length >= 10240)
                        {
                            throw new Exception("Return exceeds buffer size.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ret = ex;
                    json = JsonConvert.SerializeObject(ex);
                }
            }

            if (internalMethod?.ReturnType != typeof(void))
            {
                return json;
            }

            return null;
        }
    }
}
