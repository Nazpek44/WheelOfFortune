using System;

namespace VertigoDemo.WheelOfFortune.Events
{
    public interface IGameEvent
    {
    }

    public sealed class GameEventBus
    {
        public event Action<IGameEvent> EventRaised;

        public void Raise(IGameEvent gameEvent)
        {
            EventRaised?.Invoke(gameEvent);
        }
    }
}