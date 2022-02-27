namespace AudioPlayer.Services;

public interface IAudioService
{
    Task PlayAsync(IEnumerable<string> paths, CancellationToken cancellationToken);

    void Play(IEnumerable<string> paths);
}
