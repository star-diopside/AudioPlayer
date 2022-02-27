using AudioPlayer.Console.Services;
using AudioPlayer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    await Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddHostedService<BatchService>();
            services.AddSingleton<string[]>(args);
            services.AddSingleton<IAudioService, AudioService>();
        })
        .UseSerilog()
        .RunConsoleAsync();
}
finally
{
    Log.CloseAndFlush();
}
