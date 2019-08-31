using NAudio.Wave;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AudioPlayer
{
    static class AudioPlayer
    {
        public static async Task PlayAsync(IEnumerable<string> files)
        {
            await Task.Run(() => Play(files));
        }

        public static void Play(IEnumerable<string> files)
        {
            var lockObject = new object();

            foreach (var file in files)
            {
                try
                {
                    Log.Information("Start: {file}", file);

                    using var reader = new AudioFileReader(file);
                    using var output = new WasapiOut();

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
                    Log.Error(e, e.Message);
                }
                finally
                {
                    Log.Information("End: {file}", file);
                }
            }
        }
    }
}
