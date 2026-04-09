using Rebassed.Core.Models;

namespace Rebassed.Core.Presets;

public static class PresetLibrary
{
    public static IReadOnlyDictionary<string, RebassParameters> All { get; } =
        new Dictionary<string, RebassParameters>
        {
            ["Safe 30Hz"] = new()
            {
                TargetFrequencyHz = 30,
                SweepLowHz = 42,
                SweepHighHz = 85,
                Wide = 0.45,
                BassLevelDb = -2,
                SubsonicHpfHz = 26,
                DryWetMix = 0.35,
                OutputCeilingDb = -1.2
            },
            ["Balanced 33Hz"] = new()
            {
                TargetFrequencyHz = 33,
                SweepLowHz = 38,
                SweepHighHz = 95,
                Wide = 0.65,
                BassLevelDb = 2,
                SubsonicHpfHz = 24,
                DryWetMix = 0.45,
                OutputCeilingDb = -1
            },
            ["Deep 35Hz"] = new()
            {
                TargetFrequencyHz = 35,
                SweepLowHz = 35,
                SweepHighHz = 100,
                Wide = 0.8,
                BassLevelDb = 5,
                SubsonicHpfHz = 23,
                DryWetMix = 0.5,
                OutputCeilingDb = -1
            }
        };
}
