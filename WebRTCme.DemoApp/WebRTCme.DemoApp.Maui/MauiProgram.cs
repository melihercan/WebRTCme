using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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


        // Without this every ILogger call in the middleware goes nowhere on every MAUI platform -
        // see ConsoleLoggerProvider for what that costs when something goes wrong on a device.
        builder.Logging.AddProvider(new ConsoleLoggerProvider());

        var webRtcMiddleware = CrossWebRtcMiddlewareMaui.Current;

        builder.Services.AddSingleton(webRtcMiddleware.WebRtc);
        builder.Services.AddSingleton(webRtcMiddleware);

        builder.Services.AddMauiMiddleware();


		return builder.Build();
	}
}
