using Rebassed.Core.Models;
using Rebassed.Core.Safety;

namespace Rebassed.Core.DspEngine;

public sealed class RebassProcessor : IDspProcessor
{
    private readonly ISafetyProcessor safety;

    public RebassProcessor(ISafetyProcessor safety)
    {
        this.safety = safety;
    }

    public ProcessResult Process(AudioBuffer input, RebassParameters parameters)
    {
        SafetyValidator.Validate(parameters);

        var outChannels = new float[input.ChannelCount][];
        var bassGain = DbToLinear(parameters.BassLevelDb);

        for (var ch = 0; ch < input.ChannelCount; ch++)
        {
            var source = input.Channels[ch];
            var sub = CreateSubChannel(source, input.SampleRate, parameters);

            var mixed = new float[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                var wet = source[i] + sub[i] * bassGain;
                mixed[i] = (float)(source[i] * (1.0 - parameters.DryWetMix) + wet * parameters.DryWetMix);
            }

            outChannels[ch] = mixed;
        }

        return safety.Apply(new AudioBuffer
        {
            Channels = outChannels,
            SampleRate = input.SampleRate
        }, parameters);
    }

    private static float[] CreateSubChannel(float[] source, int sampleRate, RebassParameters p)
    {
        var center = (p.SweepLowHz + p.SweepHighHz) * 0.5;
        var q = 0.5 + (1.5 * (1.0 - p.Wide));
        var bandPass = Biquad.BandPass(sampleRate, center, q);
        var lowPass = Biquad.LowPass(sampleRate, Math.Max(45, p.TargetFrequencyHz * 1.35));

        var envelope = 0.0;
        var phase = 0.0;
        var twoPi = Math.PI * 2.0;
        var output = new float[source.Length];

        for (var i = 0; i < source.Length; i++)
        {
            var band = bandPass.Process(source[i]);
            envelope = envelope * 0.995 + Math.Abs(band) * 0.005;
            var gate = envelope > DbToLinear(p.GateThresholdDb) ? 1.0 : 0.0;

            double generated = p.GenerationMethod switch
            {
                SubGenerationMethod.RectifiedNld => Math.Tanh(band * 3.0) * 0.8,
                SubGenerationMethod.EnvelopeSine => Math.Sin(phase) * envelope * 6.0,
                _ => Math.Sin(phase) * Math.Sign(band) * envelope * 6.0
            };

            phase += twoPi * (p.TargetFrequencyHz / sampleRate);
            if (phase >= twoPi)
                phase -= twoPi;

            output[i] = lowPass.Process((float)(generated * gate));
        }

        return output;
    }

    private static double DbToLinear(double db) => Math.Pow(10.0, db / 20.0);
}
