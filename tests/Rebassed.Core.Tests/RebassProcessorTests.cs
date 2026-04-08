using FluentAssertions;
using Rebassed.Core.DspEngine;
using Rebassed.Core.Models;
using Rebassed.Core.Safety;

namespace Rebassed.Core.Tests;

public sealed class RebassProcessorTests
{
    [Fact]
    public void AddsSubEnergyOnSine()
    {
        var buffer = BuildSine(60, 48000, 2.0);
        var processor = new RebassProcessor(new SafetyProcessor());

        var result = processor.Process(buffer, new RebassParameters { TargetFrequencyHz = 30, DryWetMix = 0.6 });

        result.PeakLinear.Should().BeLessThan(1.0);
        result.Processed.Channels[0].Select(Math.Abs).Average().Should().BeGreaterThan(buffer.Channels[0].Select(Math.Abs).Average());
    }

    [Fact]
    public void ThrowsForDangerousTarget()
    {
        var buffer = BuildSine(60, 44100, 1.0);
        var processor = new RebassProcessor(new SafetyProcessor());

        var call = () => processor.Process(buffer, new RebassParameters { TargetFrequencyHz = 10 });
        call.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SupportsSweepLikeInput()
    {
        const int sampleRate = 48000;
        var len = sampleRate * 2;
        var ch = new float[len];
        for (var i = 0; i < len; i++)
        {
            var t = (double)i / sampleRate;
            var hz = 35 + 80 * t / 2.0;
            ch[i] = (float)Math.Sin(2 * Math.PI * hz * t) * 0.4f;
        }

        var processor = new RebassProcessor(new SafetyProcessor());
        var result = processor.Process(new AudioBuffer { Channels = [ch], SampleRate = sampleRate }, new RebassParameters());

        result.Processed.SampleCount.Should().Be(len);
        result.ClippingDetected.Should().BeFalse();
    }

    private static AudioBuffer BuildSine(double frequency, int sampleRate, double seconds)
    {
        var len = (int)(sampleRate * seconds);
        var channel = new float[len];
        for (var i = 0; i < len; i++)
        {
            var t = (double)i / sampleRate;
            channel[i] = (float)Math.Sin(2 * Math.PI * frequency * t) * 0.35f;
        }

        return new AudioBuffer
        {
            Channels = [channel],
            SampleRate = sampleRate
        };
    }
}
