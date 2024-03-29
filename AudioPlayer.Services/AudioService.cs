﻿using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace AudioPlayer.Services;

public class AudioService : IAudioService
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

    private static IEnumerable<string> EnumerateFiles(IEnumerable<string> paths) =>
        paths.SelectMany(static path => File.Exists(path) ? new[] { path }.AsEnumerable()
            : Directory.Exists(path) ? from file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                                       orderby Path.GetDirectoryName(Path.GetFullPath(file)), Path.GetFileName(file)
                                       select file
            : throw new FileNotFoundException("File Not Found: " + path));

    private void PlayInternal(IEnumerable<string> paths, CancellationToken cancellationToken)
    {
        foreach (var file in EnumerateFiles(paths))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            PlayInternal(file, cancellationToken);
        }
    }

    private void PlayInternal(string file, CancellationToken cancellationToken)
    {
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

            var lockObject = new object();

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

                using (var tfile = TagLib.File.Create(file))
                {
                    _logger.LogInformation("Tag: {@Tag}", new
                    {
                        tfile.Tag.Track,
                        tfile.Tag.TrackCount,
                        tfile.Tag.Title,
                        tfile.Tag.Performers,
                        tfile.Tag.Disc,
                        tfile.Tag.DiscCount,
                        tfile.Tag.Album,
                        tfile.Tag.AlbumArtists
                    });
                }

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
