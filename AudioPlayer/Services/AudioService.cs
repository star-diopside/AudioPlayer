﻿using Microsoft.Extensions.Logging;
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

        public Task PlayAsync(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            return Task.Run(() => PlayInternal(paths, cancellationToken));
        }

        public void Play(IEnumerable<string> paths)
        {
            PlayInternal(paths, CancellationToken.None);
        }

        private IEnumerable<string> EnumerateFiles(IEnumerable<string> paths) =>
            paths.SelectMany(p => File.Exists(p) ? new[] { p }
                : Directory.Exists(p) ? Directory.EnumerateFiles(p, "*", SearchOption.AllDirectories)
                : throw new FileNotFoundException(p));

        private void PlayInternal(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            var lockObject = new object();

            foreach (var file in EnumerateFiles(paths))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var fileInfo = new
                {
                    FileName = Path.GetFileName(file),
                    DirectoryName = Path.GetDirectoryName(Path.GetFullPath(file))
                };

                try
                {
                    _logger.LogInformation("Start: {@File}", fileInfo);

                    using var reader = new AudioFileReader(file);
                    using var output = new WasapiOut();

                    lock (lockObject)
                    {
                        using var registration = cancellationToken.Register(() =>
                        {
                            lock (lockObject)
                            {
                                _logger.LogInformation("Canceled.");
                            }
                            output.Stop();
                        });

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
                    _logger.LogInformation("End: {@File}", fileInfo);
                }
            }
        }
    }
}
