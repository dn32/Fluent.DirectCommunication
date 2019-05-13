using Microsoft.AspNetCore.SignalR;

namespace Fluent.DirectCommunication
{
    public static class Shared
    {
        public static IHubContext<ServerHub> MonitorHub { get; set; }
    }
}
