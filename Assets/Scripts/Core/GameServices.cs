using System.Collections.Generic;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Events;

namespace VertigoDemo.WheelOfFortune.Core
{
    /// <summary>
    /// Composition root for everything that does not need to be a Unity object.
    /// The controller receives finished services through this container and
    /// depends only on the interfaces, so nothing in the gameplay layer decides
    /// which concrete implementation it gets.
    /// </summary>
    public sealed class GameServices
    {
        public IZoneService ZoneService { get; }
        public IRunRewardInventory RunInventory { get; }
        public IPlayerWallet Wallet { get; }
        public IWheelRewardResolver RewardResolver { get; }
        public IContinueCostPolicy ContinueCostPolicy { get; }
        public GameEventBus EventBus { get; }

        public GameServices(
            IZoneService zoneService,
            IRunRewardInventory runInventory,
            IPlayerWallet wallet,
            IWheelRewardResolver rewardResolver,
            IContinueCostPolicy continueCostPolicy,
            GameEventBus eventBus
        )
        {
            ZoneService = zoneService;
            RunInventory = runInventory;
            Wallet = wallet;
            RewardResolver = rewardResolver;
            ContinueCostPolicy = continueCostPolicy;
            EventBus = eventBus;
        }

        /// <summary>
        /// Builds the default service set. A missing settings asset falls back
        /// to the code defaults rather than breaking the scene.
        /// </summary>
        public static GameServices CreateDefault(GameEconomySettings settings)
        {
            RewardProgressionProfile progression = settings
                ? settings.Progression
                : RewardProgressionProfile.Default;

            ContinueCostProfile continueCost = settings
                ? settings.ContinueCost
                : ContinueCostProfile.Default;

            IReadOnlyDictionary<CurrencyType, long> startingBalances = settings
                ? settings.StartingBalances
                : new Dictionary<CurrencyType, long> { { CurrencyType.Gold, 50L } };

            IPlayerWallet wallet = new PlayerWallet(
                new PlayerPrefsWalletStorage(),
                startingBalances
            );

            IRewardScaler rewardScaler = new ZoneRewardScaler(progression);

            IWheelRewardResolver rewardResolver = new TieredWheelRewardResolver(
                rewardScaler,
                settings ? settings.RewardTierTable : null,
                new ZoneTierProgression(settings ? settings.ZonesPerTierPromotion : 1)
            );

            return new GameServices(
                new ZoneService(),
                new RunRewardInventory(),
                wallet,
                rewardResolver,
                new ZoneContinueCostPolicy(continueCost),
                new GameEventBus()
            );
        }
    }
}
