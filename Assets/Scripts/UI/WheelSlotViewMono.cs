using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class WheelSlotViewMono : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;

        private void Reset()
        {
            _iconImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_reward_icon");
            _amountText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_reward_amount_value");
        }

        /// <summary>Shows the zone-scaled reward this slot will award.</summary>
        public void SetSlice(RewardDraw draw, string labelOverride)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;

            if (_iconImage)
            {
                _iconImage.sprite = draw.Icon;
                _iconImage.enabled = draw.Icon;
                _iconImage.preserveAspect = true;
                _iconImage.raycastTarget = false;
            }

            if (_amountText)
            {
                _amountText.text = BuildLabel(draw, labelOverride);
                _amountText.raycastTarget = false;
                _amountText.enableAutoSizing = false;
            }
        }

        public void Clear()
        {
            gameObject.SetActive(false);
        }

        private static string BuildLabel(RewardDraw draw, string labelOverride)
        {
            if (draw.IsBomb)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(labelOverride))
                return labelOverride;

            return RewardTextFormatter.FormatAmount(draw.Amount);
        }
    }
}
