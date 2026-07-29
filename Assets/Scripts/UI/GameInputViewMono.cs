using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    /// <summary>
    /// Owns the persistent gameplay buttons only. The bomb popup's buttons now
    /// belong to <see cref="BombPopupViewMono"/>, so this view no longer reaches
    /// across the hierarchy into a popup it does not own.
    /// </summary>
    public sealed class GameInputViewMono : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _spinButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _collectButton;

        public event Action SpinClicked;
        public event Action ExitClicked;
        public event Action RestartClicked;
        public event Action CollectClicked;

        private void Reset()
        {
            _spinButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_spin");
            _exitButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_exit");
            _restartButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_restart");
            _collectButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_collect");
        }

        private void OnEnable()
        {
            RegisterButtons();
        }

        private void OnDisable()
        {
            UnregisterButtons();
        }

        private void RegisterButtons()
        {
            UnregisterButtons();

            if (_spinButton)
                _spinButton.onClick.AddListener(OnSpinClicked);

            if (_exitButton)
                _exitButton.onClick.AddListener(OnExitClicked);

            if (_restartButton)
                _restartButton.onClick.AddListener(OnRestartClicked);

            if (_collectButton)
                _collectButton.onClick.AddListener(OnCollectClicked);
        }

        private void UnregisterButtons()
        {
            if (_spinButton)
                _spinButton.onClick.RemoveListener(OnSpinClicked);

            if (_exitButton)
                _exitButton.onClick.RemoveListener(OnExitClicked);

            if (_restartButton)
                _restartButton.onClick.RemoveListener(OnRestartClicked);

            if (_collectButton)
                _collectButton.onClick.RemoveListener(OnCollectClicked);
        }

        private void OnSpinClicked()
        {
            SpinClicked?.Invoke();
        }

        private void OnExitClicked()
        {
            ExitClicked?.Invoke();
        }

        private void OnRestartClicked()
        {
            RestartClicked?.Invoke();
        }

        private void OnCollectClicked()
        {
            CollectClicked?.Invoke();
        }

        public void SetGameplayButtons(bool canSpin, bool canExit)
        {
            if (_spinButton)
                _spinButton.interactable = canSpin;

            if (_exitButton)
            {
                _exitButton.gameObject.SetActive(true);
                _exitButton.interactable = canExit;
            }

            if (_restartButton)
                _restartButton.interactable = true;
        }
    }
}
