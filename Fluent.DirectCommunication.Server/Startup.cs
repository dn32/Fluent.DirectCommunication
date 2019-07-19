using Fluent.DirectCommunicationPusher;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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

            var credentials = new Credentials
            {
                AppId = "819722",
                AppKey = "5569ed05c179202d39a4",
                AppSecret = "0ea48de89d8aee835eea",
                Options = new PusherServer.PusherOptions { Cluster = "mt1", Encrypted = true }
            };

            //var clientId = "presence-test-channel-async-1";
            var clientId = "SUPPORT";
            var conn = new DuplexConnection<LocalTransmissionContract, LocalContractOfReturn>(clientId, credentials);

            new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var clients = conn.GetClients();
                            Console.WriteLine($"Online clients: {string.Join(", ", clients)}");

                            if (clients.Length > 0)
                            {
                                foreach (var chann in clients)
                                {
                                    var localTransmissionContract = new LocalTransmissionContract
                                    {
                                        Name = "Client name",
                                        Destination = chann,//"CLIENT_01",
                                        Operation = "TestMethod",
                                        Sender = clientId
                                    };

                                    var tokenId = "0.1";
                                    var ret = conn.CallAndResult(localTransmissionContract, int.MaxValue, tokenId);
                                    var json = JsonConvert.SerializeObject(ret);
                                    Console.WriteLine($"Return: {json}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var json = JsonConvert.SerializeObject(ex);
                            Console.WriteLine(json);
                        }

                        Thread.Sleep(1000);
                    }
                }).Start();

        }
    }
}
