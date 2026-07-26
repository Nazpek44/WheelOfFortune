using VertigoDemo.WheelOfFortune.Data;

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
        public readonly WheelSlice Reward;

        public RewardCollectedEvent(WheelSlice reward)
        {
            Reward = reward;
        }
    }

    public readonly struct BombHitEvent : IGameEvent
    {
    }

    public readonly struct RevivedEvent : IGameEvent
    {
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