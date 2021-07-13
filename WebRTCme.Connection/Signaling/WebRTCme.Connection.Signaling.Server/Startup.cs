using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.Connection.Signaling.Server.Data;
using WebRTCme.Connection.Signaling.Server.Hubs;
using WebRTCme.Connection.Signaling.Server.TurnServerProxies;

namespace WebRTCme.Connection.Signaling.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();

            services.AddCors(policy =>
            {
                policy
                    .AddPolicy("CorsPolicy", options => options
                //.WithOrigins("https://localhost:5001")
                //.AllowCredentials()
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            services
                .AddSignalR()
                .AddMessagePackProtocol();

            services.AddSingleton<TurnServerProxyFactory>();
            services
                .AddSingleton<StunOnlyProxy>()
                .AddSingleton<ITurnServerProxy, StunOnlyProxy>(service => service.GetService<StunOnlyProxy>());
            //services
            //    .AddSingleton<XirsysProxy>()
            //    .AddSingleton<ITurnServerProxy, XirsysProxy>(service => service.GetService<XirsysProxy>());
            //services
            //    .AddSingleton<CoturnProxy>()
            //    .AddSingleton<ITurnServerProxy, CoturnProxy>(service => service.GetService<CoturnProxy>());
            //services
            //    .AddSingleton<AppRtcProxy>()
            //    .AddSingleton<ITurnServerProxy, AppRtcProxy>(service => service.GetService<AppRtcProxy>());
            //services
            //    .AddSingleton<TwilioProxy>()
            //    .AddSingleton<ITurnServerProxy, TwilioProxy>(service => service.GetService<TwilioProxy>());


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("CorsPolicy");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapHub<RoomHub>("/roomhub");
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
