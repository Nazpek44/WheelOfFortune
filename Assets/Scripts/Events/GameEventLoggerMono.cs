using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Events
{
    public sealed class GameEventLoggerMono : MonoBehaviour
    {
        [SerializeField] private bool _logEvents = true;

        private GameEventBus _eventBus;

        public void Initialize(GameEventBus gameEventBus)
        {
            Unsubscribe();

            _eventBus = gameEventBus;

            if (_eventBus != null)
                _eventBus.EventRaised += OnEventRaised;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_eventBus != null)
                _eventBus.EventRaised -= OnEventRaised;

            _eventBus = null;
        }

        private void OnEventRaised(IGameEvent gameEvent)
        {
            if (!_logEvents || gameEvent == null)
                return;

            Debug.Log($"Game Event Raised: {gameEvent.GetType().Name}", this);
        }
    }
}
