
namespace Fluent.DirectCommunicationPusher
{
    public class ContractOfReturn
    {
        public const string OPERATION_RETURN_NAME = "RETURN";

        public ContractOfReturn()
        {
        }

        public bool Sucess { get; set; } = true;
        public string Data { get; set; }
        public string Ex { get; set; }
    }
}
