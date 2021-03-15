using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebRTCme.SignallingServer.Hubs;
using WebRTCme.SignallingServer.TurnServerService;

namespace WebRTCme.SignallingServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddCors(policy =>
            {
                policy.AddPolicy("CorsPolicy", options => options
                //.WithOrigins("https://localhost:5001")
                //.AllowCredentials()
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
            });
            
            services.AddRazorPages();
            services.AddSignalR()
                .AddMessagePackProtocol();

            services.AddSingleton<TurnServerProxyFactory>();
            services
                .AddSingleton<StunOnlyProxy>()
                .AddSingleton<ITurnServerProxy, StunOnlyProxy>(service => service.GetService<StunOnlyProxy>());
            services
                .AddSingleton<XirsysProxy>()
                .AddSingleton<ITurnServerProxy, XirsysProxy>(service => service.GetService<XirsysProxy>());
            services
                .AddSingleton<CoturnProxy>()
                .AddSingleton<ITurnServerProxy, CoturnProxy>(service => service.GetService<CoturnProxy>());
            services
                .AddSingleton<AppRtcProxy>()
                .AddSingleton<ITurnServerProxy, AppRtcProxy>(service => service.GetService<AppRtcProxy>());
            services
                .AddSingleton<TwilioProxy>()
                .AddSingleton<ITurnServerProxy, TwilioProxy>(service => service.GetService<TwilioProxy>());
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, 
                // see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<RoomHub>("/roomhub");
            });
        }
    }
}
