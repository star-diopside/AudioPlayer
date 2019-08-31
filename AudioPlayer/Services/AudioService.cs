using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudioPlayer.Services
{
    class AudioService : IAudioService
    {
        private readonly ILogger<AudioService> _logger;

        public AudioService(ILogger<AudioService> logger)
        {
            _logger = logger;
        }

        public async Task PlayAsync(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            await Task.Run(() => PlayInternal(EnumerateFiles(paths), cancellationToken));
        }

        public void Play(IEnumerable<string> paths)
        {
            PlayInternal(EnumerateFiles(paths), CancellationToken.None);
        }

        private IEnumerable<string> EnumerateFiles(IEnumerable<string> paths) =>
            paths.SelectMany(p => File.Exists(p) ? new[] { p }
                : Directory.Exists(p) ? Directory.EnumerateFiles(p, "*", SearchOption.AllDirectories)
                : throw new FileNotFoundException(p));

        private void PlayInternal(IEnumerable<string> files, CancellationToken cancellationToken)
        {
            var lockObject = new object();

            foreach (var file in files)
            {
                try
                {
                    _logger.LogInformation("Start: {file}", file);

                    using var reader = new AudioFileReader(file);
                    using var output = new WasapiOut();
                    using var registration = cancellationToken.Register(output.Stop);

                    lock (lockObject)
                    {
                        output.Init(reader);
                        output.PlaybackStopped += (s, e) =>
                        {
                            lock (lockObject)
                            {
                                Monitor.PulseAll(lockObject);
                            }
                        };
                        output.Play();
                        Monitor.Wait(lockObject);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
                finally
                {
                    _logger.LogInformation("End: {file}", file);
                }
            }
        }
    }
}
