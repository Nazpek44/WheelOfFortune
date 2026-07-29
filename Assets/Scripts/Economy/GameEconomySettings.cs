using System;
using System.Collections.Generic;
using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Economy
{
    [Serializable]
    public struct StartingBalance
    {
        public CurrencyType currency;

        [Min(0)]
        public long amount;
    }

    /// <summary>
    /// Designer facing tuning for the whole economy. Held as an asset rather
    /// than as fields on the game controller so that balance changes never
    /// require touching the scene or the code.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GameEconomySettings",
        menuName = "Wheel Of Fortune/Game Economy Settings",
        order = 0
    )]
    public sealed class GameEconomySettings : ScriptableObject
    {
        [Header("Reward Progression")]
        [Tooltip("Compounding growth per zone applied to currency rewards.")]
        [Range(0f, 1f)]
        [SerializeField] private float _currencyGrowthPerZone = 0.11f;

        [Tooltip("Compounding growth per zone applied to collectible rewards.")]
        [Range(0f, 1f)]
        [SerializeField] private float _itemGrowthPerZone = 0.04f;

        [Tooltip("Upper bound on the zone multiplier before zone type bonuses.")]
        [Min(1f)]
        [SerializeField] private float _maxMultiplier = 40f;

        [Min(1f)]
        [SerializeField] private float _safeZoneBonus = 1.5f;

        [Min(1f)]
        [SerializeField] private float _superZoneBonus = 3f;

        [Header("Continue After Bomb")]
        [Tooltip("Currency debited from the player's wallet to continue a run.")]
        [SerializeField] private CurrencyType _continueCurrency = CurrencyType.Gold;

        [Min(1)]
        [SerializeField] private int _continueBaseCost = 5;

        [Tooltip("Cost growth per zone reached.")]
        [Range(0f, 1f)]
        [SerializeField] private float _continueZoneGrowth = 0.12f;

        [Tooltip("Cost multiplier for each continue already bought in the same run.")]
        [Range(1f, 5f)]
        [SerializeField] private float _continueRepeatMultiplier = 2f;

        [Min(1)]
        [SerializeField] private int _continueMaxCost = 400;

        [Header("Item Reward Tiers")]
        [Tooltip("Gift ladder for non-currency rewards. Leave empty to fall back to amount scaling.")]
        [SerializeField] private RewardTierTable _rewardTierTable;

        [Tooltip("How many zones must pass before one more slot is promoted a tier.")]
        [Min(1)]
        [SerializeField] private int _zonesPerTierPromotion = 1;

        [Header("Starting Wallet")]
        [SerializeField]
        private StartingBalance[] _startingBalances =
        {
            new StartingBalance { currency = CurrencyType.Gold, amount = 50 },
            new StartingBalance { currency = CurrencyType.Cash, amount = 0 }
        };

        public RewardProgressionProfile Progression => new RewardProgressionProfile(
            _currencyGrowthPerZone,
            _itemGrowthPerZone,
            _maxMultiplier,
            _safeZoneBonus,
            _superZoneBonus
        );

        public ContinueCostProfile ContinueCost => new ContinueCostProfile(
            _continueCurrency,
            _continueBaseCost,
            _continueZoneGrowth,
            _continueRepeatMultiplier,
            _continueMaxCost
        );

        public RewardTierTable RewardTierTable => _rewardTierTable;

        public int ZonesPerTierPromotion => _zonesPerTierPromotion;

        public IReadOnlyDictionary<CurrencyType, long> StartingBalances
        {
            get
            {
                Dictionary<CurrencyType, long> balances = new Dictionary<CurrencyType, long>();

                if (_startingBalances == null)
                    return balances;

                for (int i = 0; i < _startingBalances.Length; i++)
                {
                    StartingBalance balance = _startingBalances[i];

                    if (balance.currency == CurrencyType.None)
                        continue;

                    balances[balance.currency] = balance.amount;
                }

                return balances;
            }
        }
    }
}
