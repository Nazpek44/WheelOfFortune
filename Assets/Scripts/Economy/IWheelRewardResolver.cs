using System.Collections.Generic;
using VertigoDemo.WheelOfFortune.Data;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Produces the full face of a wheel for a zone. The whole wheel is resolved
    /// at once so that what the player sees and what the spin awards can never
    /// drift apart.
    /// </summary>
    public interface IWheelRewardResolver
    {
        void ResolveWheel(WheelConfig config, int zone, ZoneType zoneType, List<RewardDraw> destination);
    }
}
