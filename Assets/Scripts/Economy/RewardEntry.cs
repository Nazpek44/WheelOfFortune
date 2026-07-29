using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// One grouped line of a reward list: "Cash x1300", "Bronze Chest x2".
    /// Entries are typed, so a cash entry and a gold entry can never be summed
    /// into a single meaningless total.
    /// </summary>
    public sealed class RewardEntry
    {
        public string RewardId { get; }
        public string DisplayName { get; }
        public Sprite Icon { get; }
        public RewardKind Kind { get; }
        public CurrencyType Currency { get; }
        public long TotalAmount { get; private set; }

        /// <summary>1-based gift tier; 0 for currencies.</summary>
        public int Tier { get; }

        public RewardEntry(
            string rewardId,
            string displayName,
            Sprite icon,
            RewardKind kind,
            CurrencyType currency,
            long amount,
            int tier = 0
        )
        {
            RewardId = rewardId;
            DisplayName = displayName;
            Icon = icon;
            Kind = kind;
            Currency = currency;
            TotalAmount = amount;
            Tier = tier;
        }

        public bool IsCurrency => Kind == RewardKind.Currency && Currency != CurrencyType.None;

        public void AddAmount(long amount)
        {
            TotalAmount += amount;
        }
    }
}
