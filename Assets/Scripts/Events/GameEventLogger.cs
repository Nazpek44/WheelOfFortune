using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Events
{
    public sealed class GameEventLogger : MonoBehaviour
    {
        [SerializeField] private bool logEvents = true;

        private GameEventBus eventBus;

        public void Initialize(GameEventBus gameEventBus)
        {
            eventBus = gameEventBus;

            if (eventBus != null)
                eventBus.EventRaised += OnEventRaised;
        }

        private void OnDestroy()
        {
            if (eventBus != null)
                eventBus.EventRaised -= OnEventRaised;
        }

        private void OnEventRaised(IGameEvent gameEvent)
        {
            if (!logEvents || gameEvent == null)
                return;

            Debug.Log($"Game Event Raised: {gameEvent.GetType().Name}");
        }
    }
}