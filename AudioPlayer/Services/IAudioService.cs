using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AudioPlayer.Services
{
    interface IAudioService
    {
        Task PlayAsync(IEnumerable<string> paths, CancellationToken cancellationToken);

        void Play(IEnumerable<string> paths);
    }
}
