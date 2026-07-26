using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Data;

namespace VertigoDemo.WheelOfFortune.Core
{
    public interface IRewardInventory
    {
        int TotalReward { get; }
        int ItemCount { get; }
        IReadOnlyList<InventoryEntry> Entries { get; }
        IReadOnlyList<WheelSlice> Rewards { get; }

        void Add(WheelSlice reward);
        void Clear();
    }

    public sealed class InventoryEntry
    {
        public string RewardId { get; }
        public string RewardName { get; }
        public Sprite Icon { get; }
        public int TotalAmount { get; private set; }

        public InventoryEntry(string rewardId, string rewardName, Sprite icon, int amount)
        {
            RewardId = rewardId;
            RewardName = rewardName;
            Icon = icon;
            TotalAmount = amount;
        }

        public void AddAmount(int amount)
        {
            TotalAmount += amount;
        }
    }

    public sealed class RewardInventory : IRewardInventory
    {
        private readonly Dictionary<string, InventoryEntry> entriesById = new Dictionary<string, InventoryEntry>();
        private readonly List<InventoryEntry> entries = new List<InventoryEntry>();
        private readonly List<WheelSlice> rewards = new List<WheelSlice>();

        public int TotalReward { get; private set; }
        public int ItemCount { get; private set; }
        public IReadOnlyList<InventoryEntry> Entries => entries;
        public IReadOnlyList<WheelSlice> Rewards => rewards;

        public void Add(WheelSlice reward)
        {
            if (reward == null || reward.isBomb)
                return;

            rewards.Add(reward);

            int amountToAdd = reward.amount > 0 ? reward.amount : 1;

            TotalReward += amountToAdd;
            ItemCount++;

            string key = reward.InventoryKey;

            if (entriesById.TryGetValue(key, out InventoryEntry existingEntry))
            {
                existingEntry.AddAmount(amountToAdd);
                return;
            }

            InventoryEntry newEntry = new InventoryEntry(
                key,
                reward.rewardName,
                reward.icon,
                amountToAdd
            );

            entriesById.Add(key, newEntry);
            entries.Add(newEntry);
        }

        public void Clear()
        {
            entriesById.Clear();
            entries.Clear();
            rewards.Clear();

            TotalReward = 0;
            ItemCount = 0;
        }
    }
}