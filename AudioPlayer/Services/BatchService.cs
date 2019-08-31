using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace AudioPlayer.Services
{
    class BatchService : IHostedService
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly IAudioService _audioService;
        private readonly string[] _args;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public BatchService(IApplicationLifetime applicationLifetime, IAudioService audioService, string[] args)
        {
            _applicationLifetime = applicationLifetime;
            _audioService = audioService;
            _args = args;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(async () =>
            {
                await _audioService.PlayAsync(_args, _cts.Token);
                _applicationLifetime.StopApplication();
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }
    }
}
