using System.Collections.Generic;
using VertigoDemo.WheelOfFortune.Data;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Builds a wheel face from two different progression rules:
    ///
    /// * currencies (cash, gold) keep growing by the zone multiplier, and
    /// * item gifts are swapped out for higher tier gifts as zones advance.
    ///
    /// Falls back to plain amount scaling when no tier table is supplied, so the
    /// game still runs with the asset unassigned.
    /// </summary>
    public sealed class TieredWheelRewardResolver : IWheelRewardResolver
    {
        private readonly IRewardScaler _rewardScaler;
        private readonly RewardTierTable _tierTable;
        private readonly ITierProgression _tierProgression;

        public TieredWheelRewardResolver(
            IRewardScaler rewardScaler,
            RewardTierTable tierTable,
            ITierProgression tierProgression
        )
        {
            _rewardScaler = rewardScaler;
            _tierTable = tierTable;
            _tierProgression = tierProgression;
        }

        private bool HasTierTable => _tierTable && _tierTable.IsUsable && _tierProgression != null;

        public void ResolveWheel(WheelConfig config, int zone, ZoneType zoneType, List<RewardDraw> destination)
        {
            if (destination == null)
                return;

            destination.Clear();

            if (config == null || !config.HasSlices)
                return;

            int itemSlotCount = CountItemSlots(config);
            int itemSlotOrdinal = 0;

            for (int i = 0; i < config.slices.Length; i++)
            {
                WheelSlice slice = config.slices[i];

                if (IsTieredItemSlot(slice))
                {
                    destination.Add(ResolveTieredItem(slice, zone, itemSlotOrdinal, itemSlotCount));
                    itemSlotOrdinal++;
                    continue;
                }

                destination.Add(_rewardScaler.Resolve(slice, zone, zoneType));
            }
        }

        private RewardDraw ResolveTieredItem(WheelSlice slice, int zone, int slotOrdinal, int slotCount)
        {
            int tierIndex = _tierProgression.GetTierIndex(zone, slotOrdinal, slotCount, _tierTable.TierCount);
            TieredReward reward = _tierTable.GetReward(tierIndex, slotOrdinal);

            if (reward == null)
                return _rewardScaler.Resolve(slice, zone, ZoneType.Normal);

            return new RewardDraw(
                string.IsNullOrWhiteSpace(reward.rewardId) ? "tier_reward" : reward.rewardId,
                string.IsNullOrWhiteSpace(reward.displayName) ? reward.rewardId : reward.displayName,
                reward.icon,
                RewardKind.Item,
                CurrencyType.None,
                reward.amount > 0 ? reward.amount : 1,
                false,
                tierIndex + 1
            );
        }

        private bool IsTieredItemSlot(WheelSlice slice)
        {
            if (!HasTierTable)
                return false;

            return slice != null && !slice.isBomb && slice.kind == RewardKind.Item;
        }

        private int CountItemSlots(WheelConfig config)
        {
            int count = 0;

            for (int i = 0; i < config.slices.Length; i++)
            {
                if (IsTieredItemSlot(config.slices[i]))
                    count++;
            }

            return count;
        }
    }
}
