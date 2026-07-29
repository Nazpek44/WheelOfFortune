using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class InventoryItemViewMono : MonoBehaviour
    {
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TMP_Text _amountText;

        private void Reset()
        {
            _rewardIcon = ComponentFinder.FindChildByName<Image>(this, "ui_image_inventory_reward_icon");
            _amountText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_inventory_reward_amount_value");
        }

        public void SetData(RewardEntry entry)
        {
            if (entry == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_rewardIcon)
            {
                _rewardIcon.sprite = entry.Icon;
                _rewardIcon.enabled = entry.Icon;
                _rewardIcon.preserveAspect = true;
                _rewardIcon.raycastTarget = false;
            }

            if (_amountText)
            {
                _amountText.text = RewardTextFormatter.FormatBalance(entry.TotalAmount);
                _amountText.raycastTarget = false;
            }
        }
    }
}
