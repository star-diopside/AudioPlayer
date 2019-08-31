using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioPlayer.Services
{
    interface IAudioService
    {
        Task PlayAsync(IEnumerable<string> paths);

        void Play(IEnumerable<string> paths);
    }
}
