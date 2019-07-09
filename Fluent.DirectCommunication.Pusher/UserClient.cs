
namespace Fluent.DirectCommunicationPusher
{
    public class UserClient
    {
        public string OperationExecutionId { get; set; }

        public string Client { get; }

        public string Channel { get; set; }

        public object ReturnMethod { get; internal set; }

        public UserClient(string cannel)
        {
            Channel = cannel;
        }
    }
}
