using AudioPlayer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading.Tasks;

namespace AudioPlayer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .CreateLogger();
                    logging.AddSerilog();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<BatchService>();
                    services.AddSingleton<string[]>(args);
                    services.AddSingleton<IAudioService, AudioService>();
                })
                .RunConsoleAsync();
        }
    }
}
