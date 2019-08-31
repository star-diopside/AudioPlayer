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
        private readonly object _lockObject = new object();
        private bool _stopped;

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
                _stopped = false;

                await _audioService.PlayAsync(_args, _cts.Token);

                lock (_lockObject)
                {
                    _stopped = true;
                    Monitor.PulseAll(_lockObject);
                }

                _applicationLifetime.StopApplication();
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                lock (_lockObject)
                {
                    if (!_stopped)
                    {
                        _cts.Cancel();
                        Monitor.Wait(_lockObject);
                    }
                }
            });
        }
    }
}
