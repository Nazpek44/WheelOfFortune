using System.Collections;
using TMPro;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Core;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class ZoneInfoPanelViewMono : MonoBehaviour
    {
        [SerializeField] private TMP_Text _safeZoneNumberText;
        [SerializeField] private TMP_Text _superZoneNumberText;
        [SerializeField] private GameObject _safeZonePanel;
        [SerializeField] private GameObject _superZonePanel;

        [Header("Animation")]
        [Range(1f, 2f)]
        [SerializeField] private float _activeScale = 1.08f;

        [Range(0.05f, 1f)]
        [SerializeField] private float _scaleDuration = 0.22f;

        private Coroutine _safeScaleCoroutine;
        private Coroutine _superScaleCoroutine;

        private void Reset()
        {
            _safeZoneNumberText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_safe_zone_number_value");
            _superZoneNumberText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_super_zone_number_value");
            _safeZonePanel = ComponentFinder.FindGameObjectByName(this, "ui_panel_safe_zone_info");
            _superZonePanel = ComponentFinder.FindGameObjectByName(this, "ui_panel_super_zone_info");
        }

        private void OnDestroy()
        {
            StopScaleCoroutines();
        }

        public void Refresh(IZoneService zoneService, int currentZone, ZoneType currentZoneType)
        {
            if (zoneService == null)
                return;

            if (_safeZoneNumberText)
                _safeZoneNumberText.text = zoneService.GetNextSafeZone(currentZone).ToString();

            if (_superZoneNumberText)
                _superZoneNumberText.text = zoneService.GetNextSuperZone(currentZone).ToString();

            AnimateSafePanel(currentZoneType == ZoneType.Safe ? _activeScale : 1f);
            AnimateSuperPanel(currentZoneType == ZoneType.Super ? _activeScale : 1f);
        }

        private void AnimateSafePanel(float targetScale)
        {
            if (!_safeZonePanel)
                return;

            if (_safeScaleCoroutine != null)
                StopCoroutine(_safeScaleCoroutine);

            if (!isActiveAndEnabled)
            {
                _safeZonePanel.transform.localScale = Vector3.one * targetScale;
                return;
            }

            _safeScaleCoroutine = StartCoroutine(ScaleCoroutine(_safeZonePanel.transform, targetScale));
        }

        private void AnimateSuperPanel(float targetScale)
        {
            if (!_superZonePanel)
                return;

            if (_superScaleCoroutine != null)
                StopCoroutine(_superScaleCoroutine);

            if (!isActiveAndEnabled)
            {
                _superZonePanel.transform.localScale = Vector3.one * targetScale;
                return;
            }

            _superScaleCoroutine = StartCoroutine(ScaleCoroutine(_superZonePanel.transform, targetScale));
        }

        private IEnumerator ScaleCoroutine(Transform target, float targetScale)
        {
            if (!target)
                yield break;

            Vector3 startScale = target.localScale;
            Vector3 endScale = Vector3.one * targetScale;

            float timer = 0f;

            while (timer < _scaleDuration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / _scaleDuration);

                target.localScale = Vector3.Lerp(startScale, endScale, EaseOutCubic(progress));

                yield return null;
            }

            target.localScale = endScale;
        }

        private void StopScaleCoroutines()
        {
            if (_safeScaleCoroutine != null)
                StopCoroutine(_safeScaleCoroutine);

            if (_superScaleCoroutine != null)
                StopCoroutine(_superScaleCoroutine);

            _safeScaleCoroutine = null;
            _superScaleCoroutine = null;
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
