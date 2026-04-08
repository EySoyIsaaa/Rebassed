namespace Rebassed.Core.Models;

public sealed class AudioBuffer
{
    public required float[][] Channels { get; init; }
    public required int SampleRate { get; init; }
    public int ChannelCount => Channels.Length;
    public int SampleCount => Channels.Length == 0 ? 0 : Channels[0].Length;
}
