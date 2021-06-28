using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoyalCode.Tasks.Tests.StarvationApi1.Services;
using System;

namespace RoyalCode.Tasks.Tests.StarvationApi1
{
    public class Startup1
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHttpClient("app2", client =>
            {
                client.BaseAddress = new Uri("http://localhost:2002");
            });

            services.AddTransient<App2Service>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
