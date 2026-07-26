using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class GameInputView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button spinButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button bombRestartButton;
        [SerializeField] private Button bombReviveButton;
        [SerializeField] private Button collectButton;

        public event Action SpinClicked;
        public event Action ExitClicked;
        public event Action RestartClicked;
        public event Action BombRestartClicked;
        public event Action BombReviveClicked;
        public event Action CollectClicked;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            RegisterButtons();
        }

        private void OnDisable()
        {
            UnregisterButtons();
        }

        private void CacheReferences()
        {
            if (spinButton == null)
                spinButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_spin");

            if (exitButton == null)
                exitButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_exit");

            if (restartButton == null)
                restartButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_restart");

            if (bombRestartButton == null)
                bombRestartButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_bomb_restart");

            if (bombReviveButton == null)
                bombReviveButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_bomb_revive");

            if (collectButton == null)
                collectButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_collect");

            if (collectButton == null)
                collectButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_win_collect");
        }

        private void RegisterButtons()
        {
            UnregisterButtons();

            if (spinButton != null)
                spinButton.onClick.AddListener(OnSpinClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (bombRestartButton != null)
                bombRestartButton.onClick.AddListener(OnBombRestartClicked);

            if (bombReviveButton != null)
                bombReviveButton.onClick.AddListener(OnBombReviveClicked);

            if (collectButton != null)
                collectButton.onClick.AddListener(OnCollectClicked);
        }

        private void UnregisterButtons()
        {
            if (spinButton != null)
                spinButton.onClick.RemoveListener(OnSpinClicked);

            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (bombRestartButton != null)
                bombRestartButton.onClick.RemoveListener(OnBombRestartClicked);

            if (bombReviveButton != null)
                bombReviveButton.onClick.RemoveListener(OnBombReviveClicked);

            if (collectButton != null)
                collectButton.onClick.RemoveListener(OnCollectClicked);
        }

        private void OnSpinClicked()
        {
            SpinClicked?.Invoke();
        }

        private void OnExitClicked()
        {
            Debug.Log("EXIT button clicked.");
            ExitClicked?.Invoke();
        }

        private void OnRestartClicked()
        {
            RestartClicked?.Invoke();
        }

        private void OnBombRestartClicked()
        {
            BombRestartClicked?.Invoke();
        }

        private void OnBombReviveClicked()
        {
            BombReviveClicked?.Invoke();
        }

        private void OnCollectClicked()
        {
            CollectClicked?.Invoke();
        }

        public void SetGameplayButtons(bool canSpin, bool canExit)
        {
            if (spinButton != null)
                spinButton.interactable = canSpin;

            if (exitButton != null)
            {
                exitButton.gameObject.SetActive(true);
                exitButton.interactable = canExit;
            }

            if (restartButton != null)
                restartButton.interactable = true;
        }
    }
}