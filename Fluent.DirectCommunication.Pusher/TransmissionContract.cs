
namespace Fluent.DirectCommunicationPusher
{
    public class TransmissionContract
    {
        public string Sender { get; set; }
        public string Destination { get; set; }
        public string Operation { get; set; }
        public string ReturnIdChannel { get; set; }
    }

    public class ContractOfReturn
    {
        public const string OPERATION_RETURN_NAME = "RETURN";

        public ContractOfReturn()
        {
        }

        public bool Sucess { get; set; }
        public string Ex { get; set; }
    }

    public interface IRequestController
    {
        ContractOfReturn Invoke(TransmissionContract data);
    }
}
