using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using WebRTCme.Middleware;
using Xamarinme;
namespace WebRTCme.DemoApp.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			})
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler(typeof(Media), typeof(MediaHandler));
			});

		builder.Configuration.AddConfiguration(
			new ConfigurationBuilder()
			.AddEmbeddedResource(new EmbeddedResourceConfigurationOptions
			{
                Assembly = Assembly.GetExecutingAssembly(),
                Prefix = "WebRTCme.DemoApp.Maui"
            })
			.Build());


        var webRtcMiddleware = CrossWebRtcMiddlewareMaui.Current;

        builder.Services.AddSingleton(webRtcMiddleware.WebRtc);
        builder.Services.AddSingleton(webRtcMiddleware);

        builder.Services.AddMauiMiddleware();


		return builder.Build();
	}
}
