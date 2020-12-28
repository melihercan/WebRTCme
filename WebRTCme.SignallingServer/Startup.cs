using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.SignallingServer.Data;
using WebRTCme.SignallingServer.Hubs;
using WebRTCme.SignallingServer.TurnServerService;

namespace WebRTCme.SignallingServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();

            services.AddSignalR().AddMessagePackProtocol();

            services.AddSingleton<TurnServerClientFactory>();
            services
                .AddSingleton<XirsysClient>()
                .AddSingleton<ITurnServerClient, XirsysClient>(service => service.GetService<XirsysClient>());
            services
                .AddSingleton<CoturnClient>()
                .AddSingleton<ITurnServerClient, CoturnClient>(service => service.GetService<CoturnClient>());
            services
                .AddSingleton<AppRtcClient>()
                .AddSingleton<ITurnServerClient, AppRtcClient>(service => service.GetService<AppRtcClient>());
            services
                .AddSingleton<TwilioClient>()
                .AddSingleton<ITurnServerClient, TwilioClient>(service => service.GetService<TwilioClient>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<RoomHub>("/roomhub");
            });
        }
    }
}
