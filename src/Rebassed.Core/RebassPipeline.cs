using Rebassed.Core.AudioIO;
using Rebassed.Core.DspEngine;
using Rebassed.Core.Models;

namespace Rebassed.Core;

public sealed class RebassPipeline
{
    private readonly IAudioCodec codec;
    private readonly IDspProcessor dsp;

    public RebassPipeline(IAudioCodec codec, IDspProcessor dsp)
    {
        this.codec = codec;
        this.dsp = dsp;
    }

    public async Task<ProcessResult> ProcessFileAsync(string inputMp3, string outputMp3, RebassParameters parameters, CancellationToken cancellationToken = default)
    {
        var input = await codec.DecodeMp3Async(inputMp3, cancellationToken);
        if (input.SampleRate is not (44100 or 48000))
            throw new InvalidOperationException("Only 44.1 kHz and 48 kHz MP3 files are currently supported.");

        var result = dsp.Process(input, parameters);
        await codec.EncodeMp3Async(result.Processed, outputMp3, cancellationToken: cancellationToken);
        return result;
    }
}
