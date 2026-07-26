using System.Collections;
using TMPro;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class ZoneInfoPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text safeZoneNumberText_value;
        [SerializeField] private TMP_Text superZoneNumberText_value;
        [SerializeField] private GameObject safeZonePanel;
        [SerializeField] private GameObject superZonePanel;

        [Header("Animation")]
        [SerializeField] private float activeScale = 1.08f;
        [SerializeField] private float scaleDuration = 0.22f;

        private Coroutine safeScaleRoutine;
        private Coroutine superScaleRoutine;

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
            if (safeZoneNumberText_value == null)
                safeZoneNumberText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_safe_zone_number_value");

            if (superZoneNumberText_value == null)
                superZoneNumberText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_super_zone_number_value");

            if (safeZonePanel == null)
                safeZonePanel = ComponentFinder.FindGameObjectByName(this, "ui_panel_safe_zone_info");

            if (superZonePanel == null)
                superZonePanel = ComponentFinder.FindGameObjectByName(this, "ui_panel_super_zone_info");
        }

        public void Refresh(int currentZone, ZoneType currentZoneType)
        {
            int nextSafeZone = GetNextMultiple(currentZone, GameConstants.SafeZoneInterval);
            int nextSuperZone = GetNextMultiple(currentZone, GameConstants.SuperZoneInterval);

            if (safeZoneNumberText_value != null)
                safeZoneNumberText_value.text = nextSafeZone.ToString();

            if (superZoneNumberText_value != null)
                superZoneNumberText_value.text = nextSuperZone.ToString();

            bool isSafeZone = currentZoneType == ZoneType.Safe;
            bool isSuperZone = currentZoneType == ZoneType.Super;

            AnimateSafePanel(isSafeZone ? activeScale : 1f);
            AnimateSuperPanel(isSuperZone ? activeScale : 1f);
        }

        private void AnimateSafePanel(float targetScale)
        {
            if (safeZonePanel == null)
                return;

            if (safeScaleRoutine != null)
                StopCoroutine(safeScaleRoutine);

            safeScaleRoutine = StartCoroutine(ScaleRoutine(safeZonePanel.transform, targetScale));
        }

        private void AnimateSuperPanel(float targetScale)
        {
            if (superZonePanel == null)
                return;

            if (superScaleRoutine != null)
                StopCoroutine(superScaleRoutine);

            superScaleRoutine = StartCoroutine(ScaleRoutine(superZonePanel.transform, targetScale));
        }

        private IEnumerator ScaleRoutine(Transform target, float targetScale)
        {
            Vector3 startScale = target.localScale;
            Vector3 endScale = Vector3.one * targetScale;

            float timer = 0f;

            while (timer < scaleDuration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / scaleDuration);
                float eased = EaseOutCubic(t);

                target.localScale = Vector3.Lerp(startScale, endScale, eased);

                yield return null;
            }

            target.localScale = endScale;
        }

        private int GetNextMultiple(int currentValue, int interval)
        {
            if (currentValue % interval == 0)
                return currentValue;

            return ((currentValue / interval) + 1) * interval;
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}