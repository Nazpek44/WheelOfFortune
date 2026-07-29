using System;
using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>One concrete gift that can appear on a wheel slot.</summary>
    [Serializable]
    public sealed class TieredReward
    {
        public string rewardId;
        public string displayName;
        public Sprite icon;

        [Min(1)]
        public int amount = 1;
    }

    /// <summary>A quality band of gifts. Higher bands replace lower ones as zones advance.</summary>
    [Serializable]
    public sealed class RewardTier
    {
        public string tierName;
        public TieredReward[] rewards = Array.Empty<TieredReward>();

        public bool HasRewards => rewards != null && rewards.Length > 0;
    }

    /// <summary>
    /// The gift ladder. Item rewards are not multiplied by the zone the way
    /// currencies are: they are *replaced* by better gifts, so zone 20 hands out
    /// a Gold Chest rather than "Bronze Chest x9".
    /// </summary>
    [CreateAssetMenu(
        fileName = "RewardTierTable",
        menuName = "Wheel Of Fortune/Reward Tier Table",
        order = 1
    )]
    public sealed class RewardTierTable : ScriptableObject
    {
        [Tooltip("Ordered worst to best. Index 0 is tier 1.")]
        [SerializeField] private RewardTier[] _tiers = Array.Empty<RewardTier>();

        public int TierCount => _tiers == null ? 0 : _tiers.Length;

        public bool IsUsable => TierCount > 0;

        public string GetTierName(int tierIndex)
        {
            RewardTier tier = GetTier(tierIndex);

            if (tier == null || string.IsNullOrWhiteSpace(tier.tierName))
                return "TIER " + (tierIndex + 1);

            return tier.tierName;
        }

        /// <summary>
        /// Picks a gift from a tier for a given slot. Deterministic, so the face
        /// of the wheel does not change between being drawn and being spun.
        /// </summary>
        public TieredReward GetReward(int tierIndex, int slotOrdinal)
        {
            RewardTier tier = GetTier(tierIndex);

            // Walk down to the nearest populated tier rather than returning null.
            while (tier != null && !tier.HasRewards && tierIndex > 0)
            {
                tierIndex--;
                tier = GetTier(tierIndex);
            }

            if (tier == null || !tier.HasRewards)
                return null;

            int index = slotOrdinal % tier.rewards.Length;

            if (index < 0)
                index += tier.rewards.Length;

            return tier.rewards[index];
        }

        private RewardTier GetTier(int tierIndex)
        {
            if (_tiers == null || _tiers.Length == 0)
                return null;

            int clamped = Mathf.Clamp(tierIndex, 0, _tiers.Length - 1);

            return _tiers[clamped];
        }
    }
}
