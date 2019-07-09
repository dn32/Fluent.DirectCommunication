using Fluent.DirectCommunicationPusher;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Fluent.DirectCommunication.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSignalR(routes =>
            {
                routes.MapHub<FluentServerHub>("/FluentServerHub");
            });

            var channel = "CH1_SERVER";
            var conn = new DuplexConnection<ServerOperations>(channel);

            new Thread(() =>
                {
                    while (true)
                    {
                        var channels = conn.GetCannels();
                        Console.WriteLine($"Clientes online: {string.Join(", ", channels)}");

                        if (channels.Length > 0)
                        {
                            foreach (var chann in channels)
                            {
                                var data = new Client { Name = "Client name" };
                                var ret = conn.CallAndResult(chann, "TestMethod", data, int.MaxValue);
                                Console.WriteLine(ret);
                            }
                        }

                        Thread.Sleep(1000);
                    }
                }).Start();

        }
    }

    class Client
    {
        public string Name { get; set; }
    }
}
