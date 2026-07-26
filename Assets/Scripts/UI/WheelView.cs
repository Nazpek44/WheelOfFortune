using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class WheelView : MonoBehaviour
    {
        private static readonly int AnglePropertyId = Shader.PropertyToID("_Angle");

        [SerializeField] private Image wheelBaseImage;
        [SerializeField] private Image indicatorImage;
        [SerializeField] private RawImage spinVisualRawImage;
        [SerializeField] private Material spinVisualMaterialTemplate;
        [SerializeField] private RectTransform wheelRotator;
        [SerializeField] private WheelSlotView[] slots;

        private Material runtimeSpinMaterial;

        public RectTransform WheelRotator => wheelRotator;
        public Material RuntimeSpinMaterial => runtimeSpinMaterial;

        private void Awake()
        {
            CacheReferences();
            PrepareRuntimeMaterial();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (wheelBaseImage == null)
                wheelBaseImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_spin_base");

            if (indicatorImage == null)
                indicatorImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_spin_indicator");

            if (spinVisualRawImage == null)
                spinVisualRawImage = ComponentFinder.FindChildByName<RawImage>(this, "ui_rawimage_spin_visual");

            if (wheelRotator == null)
                wheelRotator = ComponentFinder.FindChildByName<RectTransform>(this, "ui_transform_spin_rotator");

            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<WheelSlotView>(true);

            if (slots != null)
                Array.Sort(slots, (a, b) => string.CompareOrdinal(a.name, b.name));
        }

        private void PrepareRuntimeMaterial()
        {
            if (spinVisualRawImage == null || spinVisualMaterialTemplate == null)
                return;

            if (runtimeSpinMaterial == null)
                runtimeSpinMaterial = Instantiate(spinVisualMaterialTemplate);

            spinVisualRawImage.material = runtimeSpinMaterial;
        }

        public void SetWheel(WheelConfig config)
        {
            if (config == null)
                return;

            if (wheelBaseImage != null)
            {
                wheelBaseImage.sprite = config.wheelBaseSprite;
                wheelBaseImage.raycastTarget = false;
            }

            if (indicatorImage != null)
            {
                indicatorImage.sprite = config.indicatorSprite;
                indicatorImage.raycastTarget = false;
            }

            if (slots == null)
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                    continue;

                if (config.slices != null && i < config.slices.Length)
                    slots[i].SetSlice(config.slices[i]);
                else
                    slots[i].Clear();
            }
        }
    }
}