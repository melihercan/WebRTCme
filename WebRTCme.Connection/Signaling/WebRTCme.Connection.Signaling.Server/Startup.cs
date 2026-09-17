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
using WebRTCme.Connection.Signaling.Server.Hubs;
using WebRTCme.Connection.Signaling.Server.Enums;
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

            // Which TURN proxy answers "what ICE servers should I use?", from configuration.
            //
            // It used to take three edits to change, and missing any of them failed differently:
            // uncomment a registration here, change a hardcoded argument in RoomHub, and - for the
            // only hosted proxy that is implemented - remember that IHttpClientFactory was never
            // registered at all. The TURN settings in appsettings.json were consequently read by
            // nothing, which is issue #42: a deployment that works on a LAN and fails for anyone
            // behind symmetric NAT, silently, because STUN alone cannot relay.
            //
            // One key now, and RoomHub takes whatever this produces.
            var turnServer = Configuration.GetValue("SignalingServer:TurnServer", TurnServer.StunOnly);

            if (turnServer is TurnServer.Coturn or TurnServer.AppRct or TurnServer.Twilio)
                throw new NotSupportedException(
                    $"SignalingServer:TurnServer is '{turnServer}', whose proxy is a stub that " +
                    "throws NotImplementedException when a client asks for ICE servers. " +
                    "Implemented today: StunOnly (no TURN, the default) and Xirsys. Failing here " +
                    "rather than on the first client to join a room.");

            // Xirsys fetches short-lived credentials over HTTP. Nothing registered this before, so
            // selecting it would have failed to resolve even with the two gates above dealt with.
            services.AddHttpClient();

            services.AddSingleton<TurnServerProxyFactory>();
            services.AddSingleton<StunOnlyProxy>();
            services.AddSingleton<XirsysProxy>();
            services.AddSingleton<CoturnProxy>();
            services.AddSingleton<AppRtcProxy>();
            services.AddSingleton<TwilioProxy>();

            // Resolved through the factory, so the choice lives in exactly one place.
            services.AddSingleton<ITurnServerProxy>(service =>
                service.GetRequiredService<TurnServerProxyFactory>().Create(turnServer));


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
