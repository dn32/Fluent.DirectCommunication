
namespace Fluent.DirectCommunicationPusher
{
    public class TransmissionContract
    {
        public string Sender { get; set; }
        public string Destination { get; set; }
        public string Operation { get; set; }
        public string ReturnIdChannel { get; set; }
        public string Data { get; set; }
    }
}
