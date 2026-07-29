namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Tuning data for zone based reward growth. Plain data on purpose: the
    /// services that consume it never see a ScriptableObject, which keeps them
    /// testable outside the Editor.
    /// </summary>
    public readonly struct RewardProgressionProfile
    {
        public readonly float CurrencyGrowthPerZone;
        public readonly float ItemGrowthPerZone;
        public readonly float MaxMultiplier;
        public readonly float SafeZoneBonus;
        public readonly float SuperZoneBonus;

        public RewardProgressionProfile(
            float currencyGrowthPerZone,
            float itemGrowthPerZone,
            float maxMultiplier,
            float safeZoneBonus,
            float superZoneBonus
        )
        {
            CurrencyGrowthPerZone = currencyGrowthPerZone;
            ItemGrowthPerZone = itemGrowthPerZone;
            MaxMultiplier = maxMultiplier;
            SafeZoneBonus = safeZoneBonus;
            SuperZoneBonus = superZoneBonus;
        }

        /// <summary>Used when no settings asset is assigned, so the game still runs.</summary>
        public static RewardProgressionProfile Default =>
            new RewardProgressionProfile(0.11f, 0.04f, 40f, 1.5f, 3f);
    }

    /// <summary>
    /// Tuning data for the paid "continue" that lets a player survive a bomb.
    /// </summary>
    public readonly struct ContinueCostProfile
    {
        public readonly CurrencyType Currency;
        public readonly int BaseCost;
        public readonly float ZoneGrowth;
        public readonly float RepeatMultiplier;
        public readonly int MaxCost;

        public ContinueCostProfile(
            CurrencyType currency,
            int baseCost,
            float zoneGrowth,
            float repeatMultiplier,
            int maxCost
        )
        {
            Currency = currency;
            BaseCost = baseCost;
            ZoneGrowth = zoneGrowth;
            RepeatMultiplier = repeatMultiplier;
            MaxCost = maxCost;
        }

        public static ContinueCostProfile Default =>
            new ContinueCostProfile(CurrencyType.Gold, 5, 0.12f, 2f, 400);
    }
}
