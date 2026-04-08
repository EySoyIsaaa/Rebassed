using Rebassed.Core;
using Rebassed.Core.AudioIO;
using Rebassed.Core.DspEngine;
using Rebassed.Core.Models;
using Rebassed.Core.Safety;

namespace Rebassed.Desktop.Services;

/// <summary>
/// Short-preview path that processes only a window (default 10 seconds)
/// to keep UI iteration latency low.
/// </summary>
public sealed class PreviewService
{
    private readonly IAudioCodec codec = new NaudioMp3Codec();
    private readonly RebassProcessor processor = new(new SafetyProcessor());

    public async Task<ProcessResult> ProcessPreviewAsync(string inputMp3, RebassParameters parameters, double seconds = 10)
    {
        var input = await codec.DecodeMp3Async(inputMp3);
        var maxSamples = (int)(input.SampleRate * seconds);
        var clippedChannels = input.Channels
            .Select(channel => channel.Take(Math.Min(maxSamples, channel.Length)).ToArray())
            .ToArray();

        return processor.Process(new AudioBuffer
        {
            Channels = clippedChannels,
            SampleRate = input.SampleRate
        }, parameters);
    }
}
