using UnityEngine;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Decides which gift tier a given wheel slot shows at a given zone.
    /// </summary>
    public interface ITierProgression
    {
        int GetTierIndex(int zone, int slotOrdinal, int slotCount, int tierCount);
    }

    /// <summary>
    /// Promotes one slot per zone, lowest slot first. Zone 1 shows all tier 1
    /// gifts; zone 2 shows one tier 2 gift and the rest tier 1; once every slot
    /// has reached a tier, promotion continues into the next tier up.
    /// </summary>
    public sealed class ZoneTierProgression : ITierProgression
    {
        private readonly int _zonesPerPromotion;

        public ZoneTierProgression(int zonesPerPromotion)
        {
            _zonesPerPromotion = Mathf.Max(1, zonesPerPromotion);
        }

        public int GetTierIndex(int zone, int slotOrdinal, int slotCount, int tierCount)
        {
            if (slotCount <= 0 || tierCount <= 0)
                return 0;

            int zonesElapsed = Mathf.Max(0, zone - GameConstants.FIRST_ZONE);
            int promotions = zonesElapsed / _zonesPerPromotion;

            // Slot 0 is promoted first, so the upgrade sweeps across the wheel
            // one slot at a time instead of all slots changing at once.
            int tierIndex = (promotions + slotCount - 1 - slotOrdinal) / slotCount;

            return Mathf.Clamp(tierIndex, 0, tierCount - 1);
        }
    }
}
