using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    /// <summary>
    /// The bomb popup. It states plainly that the run is lost, and offers the
    /// paid continue as the only way out. The continue button is disabled when
    /// the player cannot afford it, so the price is never cosmetic.
    /// </summary>
    public sealed class BombPopupViewMono : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _continueButtonLabel;
        [SerializeField] private Button _giveUpButton;

        [Header("Colors")]
        [SerializeField] private Color _affordableColor = Color.white;
        [SerializeField] private Color _unaffordableColor = new Color(1f, 0.45f, 0.4f);

        public event Action ContinueClicked;
        public event Action GiveUpClicked;

        private readonly StringBuilder _costBuilder = new StringBuilder();

        private void Reset()
        {
            _root = gameObject;
            _titleText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_bomb_title_value");
            _costText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_revive_title_value");
            _continueButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_bomb_revive");
            _continueButtonLabel = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_revive_button_bomb_value");
            _giveUpButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_bomb_restart");
        }

        private void Awake()
        {
            if (!_root)
                _root = gameObject;
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

            if (_continueButton)
                _continueButton.onClick.AddListener(OnContinueClicked);

            if (_giveUpButton)
                _giveUpButton.onClick.AddListener(OnGiveUpClicked);
        }

        private void UnregisterButtons()
        {
            if (_continueButton)
                _continueButton.onClick.RemoveListener(OnContinueClicked);

            if (_giveUpButton)
                _giveUpButton.onClick.RemoveListener(OnGiveUpClicked);
        }

        public void Show(CurrencyType currency, long cost, long balance, int rewardsAtRisk)
        {
            if (!_root)
                _root = gameObject;

            _root.SetActive(true);
            _root.transform.SetAsLastSibling();

            bool canAfford = balance >= cost;

            if (_titleText)
            {
                _titleText.text = rewardsAtRisk > 0
                    ? "BOOM! " + rewardsAtRisk + " REWARDS WILL BE LOST"
                    : "BOOM!";
            }

            if (_costText)
            {
                _costText.text = BuildCostText(currency, cost, balance, canAfford);
                _costText.color = canAfford ? _affordableColor : _unaffordableColor;
            }

            if (_continueButtonLabel)
                _continueButtonLabel.text = "CONTINUE  " + RewardTextFormatter.FormatCost(currency.ToString(), cost);

            if (_continueButton)
                _continueButton.interactable = canAfford;
        }

        public void Hide()
        {
            // The popup starts inactive in the scene, so Awake has not run yet
            // the first time the controller hides it: fall back to gameObject.
            if (!_root)
                _root = gameObject;

            _root.SetActive(false);
        }

        private string BuildCostText(CurrencyType currency, long cost, long balance, bool canAfford)
        {
            _costBuilder.Length = 0;

            if (!canAfford)
                _costBuilder.Append("NOT ENOUGH ").Append(currency.ToString().ToUpperInvariant()).Append(" - ");

            _costBuilder.Append("COST ").Append(RewardTextFormatter.FormatBalance(cost));
            _costBuilder.Append(" / BALANCE ").Append(RewardTextFormatter.FormatBalance(balance));

            return _costBuilder.ToString();
        }

        private void OnContinueClicked()
        {
            ContinueClicked?.Invoke();
        }

        private void OnGiveUpClicked()
        {
            GiveUpClicked?.Invoke();
        }
    }
}
