using Rebassed.Core.Models;

namespace Rebassed.Core.AudioIO;

public interface IAudioCodec
{
    Task<AudioBuffer> DecodeMp3Async(string path, CancellationToken cancellationToken = default);
    Task EncodeMp3Async(AudioBuffer buffer, string path, int bitrateKbps = 320, CancellationToken cancellationToken = default);
}
