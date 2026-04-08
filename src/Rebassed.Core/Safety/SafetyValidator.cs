using Rebassed.Core.Models;

namespace Rebassed.Core.Safety;

public static class SafetyValidator
{
    public static void Validate(RebassParameters p)
    {
        if (p.TargetFrequencyHz is < 25 or > 45)
            throw new ArgumentOutOfRangeException(nameof(p.TargetFrequencyHz), "Target Frequency must stay in 25-45 Hz.");

        if (p.DryWetMix is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(p.DryWetMix), "Dry/Wet Mix must stay in 0-1.");

        if (p.SubsonicHpfHz is < 15 or > 40)
            throw new ArgumentOutOfRangeException(nameof(p.SubsonicHpfHz), "Subsonic HPF must stay in 15-40 Hz.");

        if (p.OutputCeilingDb is > -0.1 or < -12)
            throw new ArgumentOutOfRangeException(nameof(p.OutputCeilingDb), "Output ceiling must stay in -12 to -0.1 dBFS.");
    }
}
