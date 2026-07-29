using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class WinRewardItemViewMono : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TMP_Text _rewardNameText;
        [SerializeField] private TMP_Text _rewardAmountText;
        [SerializeField] private TMP_Text _rewardLabelText;

        public CanvasGroup CanvasGroup => _canvasGroup;

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rewardIcon = ComponentFinder.FindChildByName<Image>(this, "ui_image_reward_icon");
            _rewardNameText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_reward_name_value");
            _rewardAmountText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_reward_amount_value");
            _rewardLabelText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_reward_label_value");
        }

        private void Awake()
        {
            if (!_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();

            // AddComponent is safe here but never in OnValidate, where Unity
            // warns that SendMessage cannot be called during validation.
            if (!_canvasGroup)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void SetEntry(RewardEntry entry)
        {
            if (entry == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            if (_canvasGroup)
                _canvasGroup.alpha = 1f;

            if (_rewardIcon)
            {
                _rewardIcon.sprite = entry.Icon;
                _rewardIcon.enabled = entry.Icon;
                _rewardIcon.preserveAspect = true;
                _rewardIcon.raycastTarget = false;
            }

            if (_rewardNameText)
            {
                _rewardNameText.text = entry.DisplayName;
                _rewardNameText.raycastTarget = false;
            }

            if (_rewardAmountText)
            {
                _rewardAmountText.text = RewardTextFormatter.FormatAmount(entry.TotalAmount);
                _rewardAmountText.raycastTarget = false;
            }

            if (_rewardLabelText)
            {
                _rewardLabelText.text = BuildLabel(entry);
                _rewardLabelText.raycastTarget = false;
            }
        }

        private static string BuildLabel(RewardEntry entry)
        {
            if (entry.IsCurrency)
                return "CURRENCY";

            return entry.Tier > 0 ? "TIER " + entry.Tier : "ITEM";
        }
    }
}
