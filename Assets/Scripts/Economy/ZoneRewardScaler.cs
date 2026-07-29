using UnityEngine;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Applies a compounding curve to the authored base amount:
    /// <c>amount = base * (1 + growth)^(zone - 1) * zoneTypeBonus</c>, rounded to
    /// a readable number. Currencies grow faster than collectibles so that
    /// "Cash x500" becomes "Cash x11000" while "Bronze Chest x1" only becomes
    /// "Bronze Chest x3".
    /// </summary>
    public sealed class ZoneRewardScaler : IRewardScaler
    {
        private readonly RewardProgressionProfile _profile;

        public ZoneRewardScaler(RewardProgressionProfile profile)
        {
            _profile = profile;
        }

        public RewardDraw Resolve(WheelSlice slice, int zone, ZoneType zoneType)
        {
            if (slice == null)
                return RewardDraw.Bomb("Bomb", null);

            if (slice.isBomb)
                return RewardDraw.Bomb(slice.rewardName, slice.icon);

            int baseAmount = slice.amount > 0 ? slice.amount : 1;
            int scaledAmount = slice.scaleWithZone
                ? ScaleAmount(baseAmount, zone, zoneType, slice.kind)
                : baseAmount;

            return new RewardDraw(
                slice.InventoryKey,
                slice.rewardName,
                slice.icon,
                slice.kind,
                slice.currencyType,
                scaledAmount,
                false,
                0
            );
        }

        private int ScaleAmount(int baseAmount, int zone, ZoneType zoneType, RewardKind kind)
        {
            int zoneIndex = Mathf.Max(0, zone - GameConstants.FIRST_ZONE);

            float growth = kind == RewardKind.Currency
                ? _profile.CurrencyGrowthPerZone
                : _profile.ItemGrowthPerZone;

            float multiplier = Mathf.Pow(1f + growth, zoneIndex);
            multiplier = Mathf.Min(multiplier, _profile.MaxMultiplier);
            multiplier *= GetZoneTypeBonus(zoneType);

            float scaled = baseAmount * multiplier;

            return NumberRounding.ToReadableAmount(scaled);
        }

        private float GetZoneTypeBonus(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Safe:
                    return _profile.SafeZoneBonus;

                case ZoneType.Super:
                    return _profile.SuperZoneBonus;

                default:
                    return 1f;
            }
        }
    }
}
