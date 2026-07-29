using System;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Economy;

namespace VertigoDemo.WheelOfFortune.Data
{
    /// <summary>
    /// One authored slice of a wheel. The amount stored here is the *base*
    /// amount for zone 1; the amount actually granted is produced by
    /// <see cref="IRewardScaler"/> so a single definition can serve all 30 zones.
    /// </summary>
    [Serializable]
    public class WheelSlice
    {
        public string rewardId;
        public string rewardName;
        public bool isBomb;
        public Sprite icon;

        [Tooltip("Base amount at the first zone, before zone scaling.")]
        [Min(0)]
        public int amount;

        [Tooltip("Currency rewards are spendable; items are collectibles.")]
        public RewardKind kind = RewardKind.Item;

        [Tooltip("Which currency this slice pays out. Ignored for items.")]
        public CurrencyType currencyType = CurrencyType.None;

        [Tooltip("When off, this slice always pays its base amount.")]
        public bool scaleWithZone = true;

        [Tooltip("Optional fixed label. Leave empty so the scaled amount is shown.")]
        public string labelOverride;

        public string InventoryKey
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(rewardId))
                    return NormalizeKey(rewardId);

                if (!string.IsNullOrWhiteSpace(rewardName))
                    return NormalizeKey(rewardName);

                if (icon)
                    return NormalizeKey(icon.name);

                return "reward_unknown";
            }
        }

        private static string NormalizeKey(string value)
        {
            return value
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "_")
                .Replace("-", "_");
        }
    }
}
