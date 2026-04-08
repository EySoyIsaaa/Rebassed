namespace Rebassed.Core.Models;

public sealed class ProcessResult
{
    public required AudioBuffer Processed { get; init; }
    public required double PeakLinear { get; init; }
    public bool ClippingDetected => PeakLinear > 1.0;
}
