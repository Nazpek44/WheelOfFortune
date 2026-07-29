using UnityEngine;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// The cost rises with the zone reached (there is more to protect) and
    /// doubles with every continue already bought in the same run, so a run can
    /// never be carried indefinitely by repeatedly paying.
    /// </summary>
    public sealed class ZoneContinueCostPolicy : IContinueCostPolicy
    {
        private readonly ContinueCostProfile _profile;

        public ZoneContinueCostPolicy(ContinueCostProfile profile)
        {
            _profile = profile;
        }

        public CurrencyType Currency => _profile.Currency;

        public long GetCost(int zone, int continuesUsedThisRun)
        {
            int zoneIndex = Mathf.Max(0, zone - GameConstants.FIRST_ZONE);
            int repeats = Mathf.Max(0, continuesUsedThisRun);

            float cost = _profile.BaseCost
                         * Mathf.Pow(1f + _profile.ZoneGrowth, zoneIndex)
                         * Mathf.Pow(Mathf.Max(1f, _profile.RepeatMultiplier), repeats);

            int rounded = NumberRounding.ToReadableAmount(cost);

            return Mathf.Min(rounded, Mathf.Max(1, _profile.MaxCost));
        }
    }
}
