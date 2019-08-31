using Serilog;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioPlayer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            await AudioPlayer.PlayAsync(args.SelectMany(arg => File.Exists(arg) ? new[] { arg }
                : Directory.Exists(arg) ? Directory.EnumerateFiles(arg, "*", SearchOption.AllDirectories)
                : throw new FileNotFoundException(arg)));
        }
    }
}
