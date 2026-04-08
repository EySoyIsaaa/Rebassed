using Rebassed.Core.DspEngine;
using Rebassed.Core.Models;

namespace Rebassed.Core.Safety;

public sealed class SafetyProcessor : ISafetyProcessor
{
    public ProcessResult Apply(AudioBuffer buffer, RebassParameters parameters)
    {
        var ceiling = DbToLinear(parameters.OutputCeilingDb);
        var peak = 0.0;

        for (var ch = 0; ch < buffer.ChannelCount; ch++)
        {
            var hpf = Biquad.HighPass(buffer.SampleRate, parameters.SubsonicHpfHz);
            for (var i = 0; i < buffer.SampleCount; i++)
            {
                var filtered = hpf.Process(buffer.Channels[ch][i]);
                var limited = SoftLimit(filtered, ceiling);
                buffer.Channels[ch][i] = limited;
                peak = Math.Max(peak, Math.Abs(limited));
            }
        }

        return new ProcessResult
        {
            Processed = buffer,
            PeakLinear = peak
        };
    }

    private static float SoftLimit(float sample, double ceiling)
    {
        var normalized = sample / ceiling;
        var shaped = Math.Tanh(normalized);
        return (float)(shaped * ceiling);
    }

    private static double DbToLinear(double db) => Math.Pow(10.0, db / 20.0);
}
