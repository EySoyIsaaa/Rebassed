using Rebassed.Core.Models;

namespace Rebassed.Core.DspEngine;

public interface IDspProcessor
{
    ProcessResult Process(AudioBuffer input, RebassParameters parameters);
}
