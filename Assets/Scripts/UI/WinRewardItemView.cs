using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Core;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class WinRewardItemView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TMP_Text rewardNameText_value;
        [SerializeField] private TMP_Text rewardAmountText_value;
        [SerializeField] private TMP_Text rewardLabelText_value;

        public CanvasGroup CanvasGroup => canvasGroup;

        private void Awake()
        {
            CacheReferences();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (rewardIcon == null)
                rewardIcon = ComponentFinder.FindChildByName<Image>(this, "ui_image_reward_icon");

            if (rewardNameText_value == null)
                rewardNameText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_reward_name_value");

            if (rewardAmountText_value == null)
                rewardAmountText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_reward_amount_value");

            if (rewardLabelText_value == null)
                rewardLabelText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_reward_label_value");
        }

        public void SetEntry(InventoryEntry entry)
        {
            if (entry == null)
            {
                gameObject.SetActive(false);
                return;
            }

            CacheReferences();

            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            if (rewardIcon != null)
            {
                rewardIcon.sprite = entry.Icon;
                rewardIcon.enabled = entry.Icon != null;
                rewardIcon.preserveAspect = true;
                rewardIcon.raycastTarget = false;
            }

            if (rewardNameText_value != null)
            {
                rewardNameText_value.text = entry.RewardName;
                rewardNameText_value.raycastTarget = false;
            }

            if (rewardAmountText_value != null)
            {
                rewardAmountText_value.text = "x" + entry.TotalAmount;
                rewardAmountText_value.raycastTarget = false;
            }

            if (rewardLabelText_value != null)
            {
                rewardLabelText_value.text = "REWARD";
                rewardLabelText_value.raycastTarget = false;
            }
        }
    }
}