using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class WheelViewMono : MonoBehaviour
    {
        [SerializeField] private Image _wheelBaseImage;
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private RawImage _spinVisualRawImage;
        [SerializeField] private Material _spinVisualMaterialTemplate;
        [SerializeField] private RectTransform _wheelRotator;
        [SerializeField] private WheelSlotViewMono[] _slots;

        [Header("Slot Layout")]
        [Tooltip("Distance from the wheel centre to each slot.")]
        [Min(1f)]
        [SerializeField] private float _slotRadius = GameConstants.WHEEL_SLOT_RADIUS;

        [Tooltip("Lays the slots out on an exact circle at startup.")]
        [SerializeField] private bool _applyRadialLayoutOnAwake = true;

        private Material _runtimeSpinMaterial;

        public RectTransform WheelRotator => _wheelRotator;
        public Material RuntimeSpinMaterial => _runtimeSpinMaterial;

        private void Reset()
        {
            _wheelBaseImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_spin_base");
            _indicatorImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_spin_indicator");
            _spinVisualRawImage = ComponentFinder.FindChildByName<RawImage>(this, "ui_rawimage_spin_visual");
            _wheelRotator = ComponentFinder.FindChildByName<RectTransform>(this, "ui_transform_spin_rotator");

            _slots = GetComponentsInChildren<WheelSlotViewMono>(true);
            SortSlotsByName(_slots);
        }

        private void Awake()
        {
            if (_slots == null || _slots.Length == 0)
            {
                _slots = GetComponentsInChildren<WheelSlotViewMono>(true);
                SortSlotsByName(_slots);
            }

            EnsureRotatorPivotIsCentred();

            if (_applyRadialLayoutOnAwake)
                ApplyRadialLayout();

            PrepareRuntimeMaterial();
        }

        /// <summary>
        /// The wheel only looks centred if the rotator turns about its own
        /// middle. A pivot that has drifted off 0.5,0.5 makes the whole wheel
        /// orbit instead of spin, so it is corrected rather than trusted.
        /// </summary>
        private void EnsureRotatorPivotIsCentred()
        {
            if (!_wheelRotator)
                return;

            Vector2 centre = new Vector2(0.5f, 0.5f);

            if (Mathf.Abs(_wheelRotator.pivot.x - centre.x) > 0.001f ||
                Mathf.Abs(_wheelRotator.pivot.y - centre.y) > 0.001f)
            {
                Debug.LogWarning("Wheel rotator pivot was off centre and has been corrected.", this);
                _wheelRotator.pivot = centre;
            }

            _wheelRotator.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Places every slot on an exact circle. The hand placed slots drifted by
        /// up to 30 units of radius and several degrees of angle, which is what
        /// made the ring appear to wobble off centre while spinning.
        /// </summary>
        public void ApplyRadialLayout()
        {
            if (_slots == null || _slots.Length == 0)
                return;

            int slotCount = _slots.Length;

            for (int i = 0; i < slotCount; i++)
            {
                WheelSlotViewMono slot = _slots[i];

                if (!slot)
                    continue;

                RectTransform slotRect = slot.transform as RectTransform;

                if (!slotRect)
                    continue;

                slotRect.pivot = new Vector2(0.5f, 0.5f);
                slotRect.anchorMin = new Vector2(0.5f, 0.5f);
                slotRect.anchorMax = new Vector2(0.5f, 0.5f);
                slotRect.anchoredPosition = RadialLayout.GetSlotPosition(i, slotCount, _slotRadius);
                slotRect.localRotation = Quaternion.Euler(0f, 0f, RadialLayout.GetSlotRotation(i, slotCount));
                slotRect.localScale = Vector3.one;
            }
        }

        public void ResetRotation()
        {
            if (_wheelRotator)
                _wheelRotator.localRotation = Quaternion.identity;
        }

        private void PrepareRuntimeMaterial()
        {
            if (!_spinVisualRawImage || !_spinVisualMaterialTemplate)
                return;

            if (!_runtimeSpinMaterial)
                _runtimeSpinMaterial = Instantiate(_spinVisualMaterialTemplate);

            _spinVisualRawImage.material = _runtimeSpinMaterial;
        }

        public void SetWheel(WheelConfig config, IReadOnlyList<RewardDraw> resolvedSlices)
        {
            if (config == null)
                return;

            if (_wheelBaseImage)
            {
                _wheelBaseImage.sprite = config.wheelBaseSprite;
                _wheelBaseImage.raycastTarget = false;
            }

            if (_indicatorImage)
            {
                _indicatorImage.sprite = config.indicatorSprite;
                _indicatorImage.raycastTarget = false;
            }

            if (_slots == null)
                return;

            for (int i = 0; i < _slots.Length; i++)
            {
                WheelSlotViewMono slot = _slots[i];

                if (!slot)
                    continue;

                if (resolvedSlices != null && i < resolvedSlices.Count)
                    slot.SetSlice(resolvedSlices[i], GetLabelOverride(config, i));
                else
                    slot.Clear();
            }
        }

        private static string GetLabelOverride(WheelConfig config, int index)
        {
            if (config.slices == null || index >= config.slices.Length)
                return null;

            WheelSlice slice = config.slices[index];

            return slice == null ? null : slice.labelOverride;
        }

        private static void SortSlotsByName(WheelSlotViewMono[] slots)
        {
            if (slots == null)
                return;

            Array.Sort(slots, CompareByName);
        }

        /// <summary>Named comparison instead of an inline lambda, per house rules.</summary>
        private static int CompareByName(WheelSlotViewMono left, WheelSlotViewMono right)
        {
            if (!left)
                return right ? 1 : 0;

            if (!right)
                return -1;

            return string.CompareOrdinal(left.name, right.name);
        }
    }
}
