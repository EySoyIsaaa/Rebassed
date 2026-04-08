using NAudio.Utils;
using NAudio.Wave;
using Rebassed.Core.Models;

namespace Rebassed.Core.AudioIO;

public sealed class NaudioMp3Codec : IAudioCodec
{
    public async Task<AudioBuffer> DecodeMp3Async(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("MP3 file not found.", path);

        return await Task.Run(() =>
        {
            using var reader = new AudioFileReader(path);
            var channels = reader.WaveFormat.Channels;
            var sampleRate = reader.WaveFormat.SampleRate;
            var frames = new List<float[]>();
            var tmp = new float[reader.WaveFormat.SampleRate * channels];
            int read;

            while ((read = reader.Read(tmp, 0, tmp.Length)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var clone = new float[read];
                Array.Copy(tmp, clone, read);
                frames.Add(clone);
            }

            var totalSamples = frames.Sum(x => x.Length) / channels;
            var output = new float[channels][];
            for (var ch = 0; ch < channels; ch++)
                output[ch] = new float[totalSamples];

            var index = 0;
            foreach (var block in frames)
            {
                for (var i = 0; i < block.Length; i += channels)
                {
                    for (var ch = 0; ch < channels; ch++)
                    {
                        output[ch][index] = block[i + ch];
                    }

                    index++;
                }
            }

            return new AudioBuffer
            {
                Channels = output,
                SampleRate = sampleRate
            };
        }, cancellationToken);
    }

    public async Task EncodeMp3Async(AudioBuffer buffer, string path, int bitrateKbps = 320, CancellationToken cancellationToken = default)
    {
        if (buffer.ChannelCount is < 1 or > 2)
            throw new InvalidOperationException("The MP3 encoder supports mono and stereo buffers only.");

        var interleaved = new float[buffer.SampleCount * buffer.ChannelCount];
        for (var i = 0; i < buffer.SampleCount; i++)
        {
            for (var ch = 0; ch < buffer.ChannelCount; ch++)
            {
                interleaved[i * buffer.ChannelCount + ch] = buffer.Channels[ch][i];
            }
        }

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(buffer.SampleRate, buffer.ChannelCount);
            using var temp = new MemoryStream();
            using (var writer = new WaveFileWriter(new IgnoreDisposeStream(temp), waveFormat))
            {
                writer.WriteSamples(interleaved, 0, interleaved.Length);
            }

            temp.Position = 0;
            using var reader = new WaveFileReader(temp);
            MediaFoundationEncoder.EncodeToMp3(reader, path, bitrateKbps * 1000);
        }, cancellationToken);
    }
}
