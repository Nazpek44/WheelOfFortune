using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class WheelSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
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
            if (iconImage == null)
                iconImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_reward_icon");

            if (amountText_value == null)
                amountText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_reward_amount_value");
        }

        public void SetSlice(WheelSlice slice)
        {
            if (slice == null)
            {
                Clear();
                return;
            }

            gameObject.SetActive(true);
            transform.localScale = Vector3.one;

            if (iconImage != null)
            {
                iconImage.sprite = slice.icon;
                iconImage.enabled = slice.icon != null;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
            }

            if (amountText_value != null)
            {
                amountText_value.text = slice.GetDisplayText();
                amountText_value.raycastTarget = false;
                amountText_value.enableAutoSizing = false;
            }
        }

        public void Clear()
        {
            gameObject.SetActive(false);
        }
    }
}