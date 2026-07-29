using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// A wheel slice after zone scaling has been applied. The authored
    /// <c>WheelSlice</c> stays immutable; this is what the game actually awards
    /// and what the views display, so zone 1 and zone 29 can share one slice
    /// definition while paying out very different amounts.
    /// </summary>
    public readonly struct RewardDraw
    {
        public readonly string RewardId;
        public readonly string DisplayName;
        public readonly Sprite Icon;
        public readonly RewardKind Kind;
        public readonly CurrencyType Currency;
        public readonly int Amount;
        public readonly bool IsBomb;

        /// <summary>1-based gift tier; 0 for currencies and the bomb.</summary>
        public readonly int Tier;

        public RewardDraw(
            string rewardId,
            string displayName,
            Sprite icon,
            RewardKind kind,
            CurrencyType currency,
            int amount,
            bool isBomb,
            int tier = 0
        )
        {
            RewardId = rewardId;
            DisplayName = displayName;
            Icon = icon;
            Kind = kind;
            Currency = currency;
            Amount = amount;
            IsBomb = isBomb;
            Tier = tier;
        }

        public bool IsCurrency => Kind == RewardKind.Currency && Currency != CurrencyType.None;

        public static RewardDraw Bomb(string displayName, Sprite icon)
        {
            return new RewardDraw(
                "bomb",
                displayName,
                icon,
                RewardKind.Item,
                CurrencyType.None,
                0,
                true,
                0
            );
        }
    }
}
