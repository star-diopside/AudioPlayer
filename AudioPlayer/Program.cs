using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace AudioPlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = args.SelectMany(arg => File.Exists(arg) ? new[] { arg }
                : Directory.Exists(arg) ? Directory.EnumerateFiles(arg, "*", SearchOption.AllDirectories)
                : throw new FileNotFoundException(arg));

            var lockObject = new object();

            foreach (var file in files)
            {
                try
                {
                    Console.WriteLine(file);

                    using (var reader = new AudioFileReader(file))
                    using (var output = new WasapiOut())
                    {
                        lock (lockObject)
                        {
                            output.Init(reader);
                            output.PlaybackStopped += (s, e) => { lock (lockObject) { Monitor.PulseAll(lockObject); } };
                            output.Play();
                            Monitor.Wait(lockObject);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
        }
    }
}
