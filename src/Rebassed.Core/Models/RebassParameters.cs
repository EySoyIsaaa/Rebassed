namespace Rebassed.Core.Models;

public enum SubGenerationMethod
{
    PeriodTracker = 0,
    RectifiedNld = 1,
    EnvelopeSine = 2
}

public sealed class RebassParameters
{
    public double TargetFrequencyHz { get; set; } = 33;
    public double SweepLowHz { get; set; } = 40;
    public double SweepHighHz { get; set; } = 90;
    public double Wide { get; set; } = 0.65;
    public double BassLevelDb { get; set; } = -6;
    public double SubsonicHpfHz { get; set; } = 25;
    public double DryWetMix { get; set; } = 0.4;
    public double OutputCeilingDb { get; set; } = -1;
    public double GateThresholdDb { get; set; } = -42;
    public SubGenerationMethod GenerationMethod { get; set; } = SubGenerationMethod.PeriodTracker;
}
