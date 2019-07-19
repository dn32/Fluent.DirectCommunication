
using System.Reflection;

namespace Fluent.DirectCommunicationPusher
{
    public interface IRequestController
    {
        ContractOfReturn Invoke(TransmissionContract data);
    } 

}
