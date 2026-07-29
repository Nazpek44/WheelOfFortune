using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Economy;

namespace VertigoDemo.WheelOfFortune.Events
{
    public readonly struct GameStartedEvent : IGameEvent
    {
    }

    public readonly struct SpinStartedEvent : IGameEvent
    {
    }

    public readonly struct SpinCompletedEvent : IGameEvent
    {
        public readonly int SliceIndex;

        public SpinCompletedEvent(int sliceIndex)
        {
            SliceIndex = sliceIndex;
        }
    }

    public readonly struct RewardCollectedEvent : IGameEvent
    {
        public readonly RewardDraw Reward;

        public RewardCollectedEvent(RewardDraw reward)
        {
            Reward = reward;
        }
    }

    public readonly struct BombHitEvent : IGameEvent
    {
        public readonly int Zone;

        public BombHitEvent(int zone)
        {
            Zone = zone;
        }
    }

    /// <summary>Raised when a bomb resolves without payment: the run is lost.</summary>
    public readonly struct RunLostEvent : IGameEvent
    {
        public readonly int Zone;
        public readonly int RewardsLost;

        public RunLostEvent(int zone, int rewardsLost)
        {
            Zone = zone;
            RewardsLost = rewardsLost;
        }
    }

    /// <summary>Raised when the player pays currency to survive a bomb.</summary>
    public readonly struct ContinuePurchasedEvent : IGameEvent
    {
        public readonly int Zone;
        public readonly CurrencyType Currency;
        public readonly long Cost;

        public ContinuePurchasedEvent(int zone, CurrencyType currency, long cost)
        {
            Zone = zone;
            Currency = currency;
            Cost = cost;
        }
    }

    public readonly struct ContinueRejectedEvent : IGameEvent
    {
        public readonly CurrencyType Currency;
        public readonly long Cost;
        public readonly long Balance;

        public ContinueRejectedEvent(CurrencyType currency, long cost, long balance)
        {
            Currency = currency;
            Cost = cost;
            Balance = balance;
        }
    }

    /// <summary>Raised when a run's rewards are banked into the persistent wallet.</summary>
    public readonly struct RewardsBankedEvent : IGameEvent
    {
        public readonly int EntryCount;

        public RewardsBankedEvent(int entryCount)
        {
            EntryCount = entryCount;
        }
    }

    public readonly struct GameRestartedEvent : IGameEvent
    {
    }

    public readonly struct ZoneChangedEvent : IGameEvent
    {
        public readonly int Zone;
        public readonly ZoneType ZoneType;

        public ZoneChangedEvent(int zone, ZoneType zoneType)
        {
            Zone = zone;
            ZoneType = zoneType;
        }
    }
}
