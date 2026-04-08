namespace Rebassed.Core.DspEngine;

internal sealed class Biquad
{
    private readonly double a0;
    private readonly double a1;
    private readonly double a2;
    private readonly double b1;
    private readonly double b2;

    private double x1;
    private double x2;
    private double y1;
    private double y2;

    private Biquad(double a0, double a1, double a2, double b1, double b2)
    {
        this.a0 = a0;
        this.a1 = a1;
        this.a2 = a2;
        this.b1 = b1;
        this.b2 = b2;
    }

    public static Biquad BandPass(double sampleRate, double centerHz, double q)
    {
        var w0 = 2.0 * Math.PI * centerHz / sampleRate;
        var alpha = Math.Sin(w0) / (2.0 * q);

        var b0 = alpha;
        var b1 = 0.0;
        var b2 = -alpha;
        var a0 = 1.0 + alpha;
        var a1 = -2.0 * Math.Cos(w0);
        var a2 = 1.0 - alpha;

        return Normalize(b0, b1, b2, a0, a1, a2);
    }

    public static Biquad LowPass(double sampleRate, double cutHz, double q = 0.707)
    {
        var w0 = 2.0 * Math.PI * cutHz / sampleRate;
        var alpha = Math.Sin(w0) / (2.0 * q);
        var cos = Math.Cos(w0);

        var b0 = (1 - cos) / 2;
        var b1 = 1 - cos;
        var b2 = (1 - cos) / 2;
        var a0 = 1 + alpha;
        var a1 = -2 * cos;
        var a2 = 1 - alpha;

        return Normalize(b0, b1, b2, a0, a1, a2);
    }

    public static Biquad HighPass(double sampleRate, double cutHz, double q = 0.707)
    {
        var w0 = 2.0 * Math.PI * cutHz / sampleRate;
        var alpha = Math.Sin(w0) / (2.0 * q);
        var cos = Math.Cos(w0);

        var b0 = (1 + cos) / 2;
        var b1 = -(1 + cos);
        var b2 = (1 + cos) / 2;
        var a0 = 1 + alpha;
        var a1 = -2 * cos;
        var a2 = 1 - alpha;

        return Normalize(b0, b1, b2, a0, a1, a2);
    }

    public float Process(float input)
    {
        var y = a0 * input + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;

        x2 = x1;
        x1 = input;
        y2 = y1;
        y1 = y;

        return (float)y;
    }

    private static Biquad Normalize(double b0, double b1, double b2, double a0, double a1, double a2)
    {
        return new Biquad(b0 / a0, b1 / a0, b2 / a0, a1 / a0, a2 / a0);
    }
}
