using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Core;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class InventoryItemView : MonoBehaviour
    {
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TMP_Text amountText_value;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (rewardIcon == null)
                rewardIcon = ComponentFinder.FindChildByName<Image>(this, "ui_image_inventory_reward_icon");

            if (amountText_value == null)
                amountText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_inventory_reward_amount_value");
        }

        public void SetData(InventoryEntry entry)
        {
            if (entry == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (rewardIcon != null)
            {
                rewardIcon.sprite = entry.Icon;
                rewardIcon.enabled = entry.Icon != null;
                rewardIcon.preserveAspect = true;
                rewardIcon.raycastTarget = false;
            }

            if (amountText_value != null)
            {
                amountText_value.text = entry.TotalAmount.ToString();
                amountText_value.raycastTarget = false;
            }
        }
    }
}