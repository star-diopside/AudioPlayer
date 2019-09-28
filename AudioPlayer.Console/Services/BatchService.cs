using AudioPlayer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AudioPlayer.Console.Services
{
    class BatchService : IHostedService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<BatchService> _logger;
        private readonly IAudioService _audioService;
        private readonly string[] _args;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly object _lockObject = new object();
        private bool _stopped = true;

        public BatchService(IHostApplicationLifetime applicationLifetime, ILogger<BatchService> logger,
            IAudioService audioService, string[] args)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _audioService = audioService;
            _args = args;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    _stopped = false;
                    await _audioService.PlayAsync(_args, _cts.Token);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
                finally
                {
                    lock (_lockObject)
                    {
                        _stopped = true;
                        Monitor.PulseAll(_lockObject);
                    }

                    _applicationLifetime.StopApplication();
                }
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
