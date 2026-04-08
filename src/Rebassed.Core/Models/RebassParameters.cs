using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rebassed.Core.Models;

public enum SubGenerationMethod
{
    PeriodTracker = 0,
    RectifiedNld = 1,
    EnvelopeSine = 2
}

public sealed class RebassParameters : INotifyPropertyChanged
{
    private double targetFrequencyHz = 33;
    private double sweepLowHz = 40;
    private double sweepHighHz = 90;
    private double wide = 0.65;
    private double bassLevelDb = -6;
    private double subsonicHpfHz = 25;
    private double dryWetMix = 0.4;
    private double outputCeilingDb = -1;
    private double gateThresholdDb = -42;
    private SubGenerationMethod generationMethod = SubGenerationMethod.PeriodTracker;

    public event PropertyChangedEventHandler? PropertyChanged;

    public double TargetFrequencyHz
    {
        get => targetFrequencyHz;
        set => Set(ref targetFrequencyHz, value);
    }

    public double SweepLowHz
    {
        get => sweepLowHz;
        set => Set(ref sweepLowHz, value);
    }

    public double SweepHighHz
    {
        get => sweepHighHz;
        set => Set(ref sweepHighHz, value);
    }

    public double Wide
    {
        get => wide;
        set => Set(ref wide, value);
    }

    public double BassLevelDb
    {
        get => bassLevelDb;
        set => Set(ref bassLevelDb, value);
    }

    public double SubsonicHpfHz
    {
        get => subsonicHpfHz;
        set => Set(ref subsonicHpfHz, value);
    }

    public double DryWetMix
    {
        get => dryWetMix;
        set => Set(ref dryWetMix, value);
    }

    public double OutputCeilingDb
    {
        get => outputCeilingDb;
        set => Set(ref outputCeilingDb, value);
    }

    public double GateThresholdDb
    {
        get => gateThresholdDb;
        set => Set(ref gateThresholdDb, value);
    }

    public SubGenerationMethod GenerationMethod
    {
        get => generationMethod;
        set => Set(ref generationMethod, value);
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
