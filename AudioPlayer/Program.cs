using AudioPlayer.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace AudioPlayer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            await Run(serviceProvider, args);
        }

        static void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            services.AddLogging(configure => configure.AddSerilog());

            services.AddSingleton<IAudioService, AudioService>();
        }

        static async Task Run(IServiceProvider serviceProvider, string[] args)
        {
            var audioService = serviceProvider.GetService<IAudioService>();
            await audioService.PlayAsync(args);
        }
    }
}
