using Rebassed.Core.Models;

namespace Rebassed.Core.Safety;

public interface ISafetyProcessor
{
    ProcessResult Apply(AudioBuffer buffer, RebassParameters parameters);
}
