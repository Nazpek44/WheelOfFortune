using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class ProgressBarViewMono : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _stepsContainer;
        [SerializeField] private TMP_Text[] _stepTexts;
        [SerializeField] private RectTransform _currentMarker;
        [SerializeField] private TMP_Text _currentZoneText;

        [Header("Marker Images")]
        [SerializeField] private Image[] _currentMarkerImages;

        [Header("Layout")]
        [Min(1f)]
        [SerializeField] private float _fixedStepWidth = 48f;

        [Min(1f)]
        [SerializeField] private float _fixedStepHeight = 55f;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _safeColor = Color.green;
        [SerializeField] private Color _superColor = new Color(1f, 0.75f, 0f);
        [SerializeField] private Color _currentTextColor = Color.black;
        [SerializeField] private Color _normalMarkerColor = Color.white;

        [Header("Animation")]
        [Range(0.05f, 1f)]
        [SerializeField] private float _markerMoveDuration = 0.25f;

        [Range(1f, 2f)]
        [SerializeField] private float _markerPulseScale = 1.12f;

        [Range(0.02f, 0.5f)]
        [SerializeField] private float _markerPulseDuration = 0.12f;

        private Coroutine _markerCoroutine;
        private Coroutine _pulseCoroutine;

        private void Reset()
        {
            _stepsContainer = ComponentFinder.FindChildByName<RectTransform>(this, "ui_content_progress_steps");
            _currentMarker = ComponentFinder.FindChildByName<RectTransform>(this, "ui_current_zone_indicator");
            _currentZoneText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_current_zone_value");

            if (_currentMarker)
                _currentMarkerImages = _currentMarker.GetComponentsInChildren<Image>(true);

            if (_stepsContainer)
                _stepTexts = _stepsContainer.GetComponentsInChildren<TMP_Text>(true);
        }

        private void Awake()
        {
            if (!_stepsContainer)
                _stepsContainer = ComponentFinder.FindChildByName<RectTransform>(this, "ui_content_progress_steps");

            if (_stepTexts == null || _stepTexts.Length == 0)
            {
                _stepTexts = _stepsContainer
                    ? _stepsContainer.GetComponentsInChildren<TMP_Text>(true)
                    : GetComponentsInChildren<TMP_Text>(true);
            }

            // Layout is applied once at startup rather than from OnValidate,
            // where AddComponent triggers Unity's SendMessage warning.
            ApplyFixedStepLayout();
        }

        private void OnDestroy()
        {
            StopMarkerCoroutines();
        }

        private void ApplyFixedStepLayout()
        {
            if (_stepTexts == null)
                return;

            for (int i = 0; i < _stepTexts.Length; i++)
            {
                TMP_Text stepText = _stepTexts[i];

                if (!stepText)
                    continue;

                stepText.alignment = TextAlignmentOptions.Center;
                stepText.raycastTarget = false;
                stepText.enableAutoSizing = false;

                RectTransform stepRect = stepText.transform as RectTransform;

                if (stepRect)
                    stepRect.sizeDelta = new Vector2(_fixedStepWidth, _fixedStepHeight);

                LayoutElement layoutElement = stepText.GetComponent<LayoutElement>();

                if (!layoutElement)
                    layoutElement = stepText.gameObject.AddComponent<LayoutElement>();

                layoutElement.minWidth = _fixedStepWidth;
                layoutElement.preferredWidth = _fixedStepWidth;
                layoutElement.flexibleWidth = 0f;

                layoutElement.minHeight = _fixedStepHeight;
                layoutElement.preferredHeight = _fixedStepHeight;
                layoutElement.flexibleHeight = 0f;
            }

            if (_currentZoneText)
            {
                _currentZoneText.alignment = TextAlignmentOptions.Center;
                _currentZoneText.raycastTarget = false;
                _currentZoneText.enableAutoSizing = false;
            }
        }

        public void Refresh(int currentZone, ZoneType zoneType, bool animate)
        {
            if (_stepTexts == null || _stepTexts.Length == 0)
                return;

            int visibleCount = Mathf.Min(GameConstants.VISIBLE_PROGRESS_STEP_COUNT, _stepTexts.Length);
            int startZone = Mathf.Max(GameConstants.FIRST_ZONE, currentZone - visibleCount / 2);

            for (int i = 0; i < visibleCount; i++)
            {
                TMP_Text stepText = _stepTexts[i];

                if (!stepText)
                    continue;

                int zoneNumber = startZone + i;

                stepText.text = zoneNumber.ToString();
                stepText.color = GetColorForZone(zoneNumber);
                stepText.raycastTarget = false;
            }

            if (_currentZoneText)
            {
                _currentZoneText.text = currentZone.ToString();
                _currentZoneText.color = _currentTextColor;
                _currentZoneText.raycastTarget = false;
            }

            int currentIndex = Mathf.Clamp(currentZone - startZone, 0, visibleCount - 1);

            Canvas.ForceUpdateCanvases();

            if (_stepsContainer)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_stepsContainer);

            Canvas.ForceUpdateCanvases();

            SetMarkerColor(zoneType);
            MoveMarkerToStep(currentIndex, animate);
            PlayZonePulse(zoneType);
        }

        private Color GetColorForZone(int zone)
        {
            if (zone % GameConstants.SUPER_ZONE_INTERVAL == 0)
                return _superColor;

            if (zone % GameConstants.SAFE_ZONE_INTERVAL == 0)
                return _safeColor;

            return _normalColor;
        }

        private Color GetMarkerColor(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Safe:
                    return _safeColor;

                case ZoneType.Super:
                    return _superColor;

                default:
                    return _normalMarkerColor;
            }
        }

        private void SetMarkerColor(ZoneType zoneType)
        {
            if (_currentMarkerImages == null)
                return;

            Color markerColor = GetMarkerColor(zoneType);

            for (int i = 0; i < _currentMarkerImages.Length; i++)
            {
                Image markerImage = _currentMarkerImages[i];

                if (!markerImage)
                    continue;

                markerImage.color = markerColor;
                markerImage.raycastTarget = false;
            }
        }

        private void MoveMarkerToStep(int stepIndex, bool animate)
        {
            if (!_currentMarker || _stepTexts == null)
                return;

            if (stepIndex < 0 || stepIndex >= _stepTexts.Length)
                return;

            TMP_Text targetStepText = _stepTexts[stepIndex];

            if (!targetStepText)
                return;

            RectTransform markerParent = _currentMarker.parent as RectTransform;
            RectTransform targetRect = targetStepText.transform as RectTransform;

            if (!markerParent || !targetRect)
                return;

            Vector3 targetWorldPosition = targetRect.TransformPoint(targetRect.rect.center);
            Vector3 targetLocalPosition = markerParent.InverseTransformPoint(targetWorldPosition);

            Vector2 targetAnchoredPosition = new Vector2(
                targetLocalPosition.x,
                _currentMarker.anchoredPosition.y
            );

            if (_markerCoroutine != null)
                StopCoroutine(_markerCoroutine);

            if (!animate || !isActiveAndEnabled)
            {
                _currentMarker.anchoredPosition = targetAnchoredPosition;
                return;
            }

            _markerCoroutine = StartCoroutine(MoveMarkerCoroutine(targetAnchoredPosition));
        }

        private IEnumerator MoveMarkerCoroutine(Vector2 targetPosition)
        {
            if (!_currentMarker)
                yield break;

            Vector2 startPosition = _currentMarker.anchoredPosition;
            float timer = 0f;

            while (timer < _markerMoveDuration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / _markerMoveDuration);

                _currentMarker.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, EaseOutCubic(progress));

                yield return null;
            }

            _currentMarker.anchoredPosition = targetPosition;
            _markerCoroutine = null;
        }

        private void PlayZonePulse(ZoneType zoneType)
        {
            if (!_currentMarker || !isActiveAndEnabled)
                return;

            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);

            _pulseCoroutine = StartCoroutine(PulseCoroutine(zoneType));
        }

        private IEnumerator PulseCoroutine(ZoneType zoneType)
        {
            if (!_currentMarker)
                yield break;

            SetMarkerColor(zoneType);

            Vector3 normalScale = Vector3.one;
            Vector3 targetScale = Vector3.one * _markerPulseScale;

            float timer = 0f;

            while (timer < _markerPulseDuration)
            {
                timer += Time.deltaTime;

                _currentMarker.localScale = Vector3.Lerp(
                    normalScale,
                    targetScale,
                    Mathf.Clamp01(timer / _markerPulseDuration)
                );

                yield return null;
            }

            timer = 0f;

            while (timer < _markerPulseDuration)
            {
                timer += Time.deltaTime;

                _currentMarker.localScale = Vector3.Lerp(
                    targetScale,
                    normalScale,
                    Mathf.Clamp01(timer / _markerPulseDuration)
                );

                yield return null;
            }

            _currentMarker.localScale = normalScale;

            SetMarkerColor(zoneType);

            _pulseCoroutine = null;
        }

        private void StopMarkerCoroutines()
        {
            if (_markerCoroutine != null)
                StopCoroutine(_markerCoroutine);

            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);

            _markerCoroutine = null;
            _pulseCoroutine = null;
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
