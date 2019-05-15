using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
        }
    }
}
