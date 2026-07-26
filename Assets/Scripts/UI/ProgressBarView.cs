using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class ProgressBarView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform stepsContainer;
        [SerializeField] private TMP_Text[] stepTexts_value;
        [SerializeField] private RectTransform currentMarker;
        [SerializeField] private TMP_Text currentZoneText_value;

        [Header("Marker Images")]
        [SerializeField] private Image[] currentMarkerImages;

        [Header("Layout")]
        [SerializeField] private float fixedStepWidth = 48f;
        [SerializeField] private float fixedStepHeight = 55f;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color safeColor = Color.green;
        [SerializeField] private Color superColor = new Color(1f, 0.75f, 0f);
        [SerializeField] private Color currentTextColor = Color.black;
        [SerializeField] private Color normalMarkerColor = Color.white;

        [Header("Animation")]
        [SerializeField] private float markerMoveDuration = 0.25f;
        [SerializeField] private float markerPulseScale = 1.12f;
        [SerializeField] private float markerPulseDuration = 0.12f;

        private Coroutine markerRoutine;
        private Coroutine pulseRoutine;

        private void Awake()
        {
            CacheReferences();
            ApplyFixedStepLayout();
        }

        private void OnValidate()
        {
            CacheReferences();
            ApplyFixedStepLayout();
        }

        private void CacheReferences()
        {
            if (stepsContainer == null)
                stepsContainer = ComponentFinder.FindChildByName<RectTransform>(this, "ui_content_progress_steps");

            if (currentMarker == null)
                currentMarker = ComponentFinder.FindChildByName<RectTransform>(this, "ui_current_zone_indicator");

            if (currentZoneText_value == null)
                currentZoneText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_current_zone_value");

            if ((currentMarkerImages == null || currentMarkerImages.Length == 0) && currentMarker != null)
                currentMarkerImages = currentMarker.GetComponentsInChildren<Image>(true);

            if (stepTexts_value == null || stepTexts_value.Length == 0)
            {
                if (stepsContainer != null)
                    stepTexts_value = stepsContainer.GetComponentsInChildren<TMP_Text>(true);
                else
                    stepTexts_value = GetComponentsInChildren<TMP_Text>(true);
            }
        }

        private void ApplyFixedStepLayout()
        {
            if (stepTexts_value == null)
                return;

            foreach (TMP_Text stepText in stepTexts_value)
            {
                if (stepText == null)
                    continue;

                stepText.alignment = TextAlignmentOptions.Center;
                stepText.raycastTarget = false;
                stepText.enableAutoSizing = false;

                RectTransform rectTransform = stepText.transform as RectTransform;

                if (rectTransform != null)
                    rectTransform.sizeDelta = new Vector2(fixedStepWidth, fixedStepHeight);

                LayoutElement layoutElement = stepText.GetComponent<LayoutElement>();

                if (layoutElement == null)
                    layoutElement = stepText.gameObject.AddComponent<LayoutElement>();

                layoutElement.minWidth = fixedStepWidth;
                layoutElement.preferredWidth = fixedStepWidth;
                layoutElement.flexibleWidth = 0f;

                layoutElement.minHeight = fixedStepHeight;
                layoutElement.preferredHeight = fixedStepHeight;
                layoutElement.flexibleHeight = 0f;
            }

            if (currentZoneText_value != null)
            {
                currentZoneText_value.alignment = TextAlignmentOptions.Center;
                currentZoneText_value.raycastTarget = false;
                currentZoneText_value.enableAutoSizing = false;
            }
        }

        public void Refresh(int currentZone, ZoneType zoneType, bool animate)
        {
            CacheReferences();
            ApplyFixedStepLayout();

            if (stepTexts_value == null || stepTexts_value.Length == 0)
                return;

            int visibleCount = Mathf.Min(GameConstants.VisibleProgressStepCount, stepTexts_value.Length);
            int startZone = Mathf.Max(1, currentZone - visibleCount / 2);

            for (int i = 0; i < visibleCount; i++)
            {
                int zoneNumber = startZone + i;
                TMP_Text stepText = stepTexts_value[i];

                if (stepText == null)
                    continue;

                stepText.text = zoneNumber.ToString();
                stepText.color = GetColorForZone(zoneNumber);
                stepText.raycastTarget = false;
            }

            if (currentZoneText_value != null)
            {
                currentZoneText_value.text = currentZone.ToString();
                currentZoneText_value.color = currentTextColor;
                currentZoneText_value.raycastTarget = false;
            }

            int currentIndex = Mathf.Clamp(currentZone - startZone, 0, visibleCount - 1);

            Canvas.ForceUpdateCanvases();

            if (stepsContainer != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(stepsContainer);

            Canvas.ForceUpdateCanvases();

            SetMarkerColor(zoneType);
            MoveMarkerToStep(currentIndex, animate);
            PlayZonePulse(zoneType);
        }

        private Color GetColorForZone(int zone)
        {
            if (zone % GameConstants.SuperZoneInterval == 0)
                return superColor;

            if (zone % GameConstants.SafeZoneInterval == 0)
                return safeColor;

            return normalColor;
        }

        private Color GetMarkerColor(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Safe:
                    return safeColor;

                case ZoneType.Super:
                    return superColor;

                default:
                    return normalMarkerColor;
            }
        }

        private void SetMarkerColor(ZoneType zoneType)
        {
            Color markerColor = GetMarkerColor(zoneType);

            if (currentMarkerImages == null)
                return;

            foreach (Image image in currentMarkerImages)
            {
                if (image == null)
                    continue;

                image.color = markerColor;
                image.raycastTarget = false;
            }
        }

        private void MoveMarkerToStep(int stepIndex, bool animate)
        {
            if (currentMarker == null || stepTexts_value == null)
                return;

            if (stepIndex < 0 || stepIndex >= stepTexts_value.Length)
                return;

            TMP_Text targetStepText = stepTexts_value[stepIndex];

            if (targetStepText == null)
                return;

            RectTransform markerParent = currentMarker.parent as RectTransform;
            RectTransform targetRect = targetStepText.transform as RectTransform;

            if (markerParent == null || targetRect == null)
                return;

            Vector3 targetWorldPosition = targetRect.TransformPoint(targetRect.rect.center);
            Vector3 targetLocalPosition = markerParent.InverseTransformPoint(targetWorldPosition);

            Vector2 targetAnchoredPosition = new Vector2(
                targetLocalPosition.x,
                currentMarker.anchoredPosition.y
            );

            if (markerRoutine != null)
                StopCoroutine(markerRoutine);

            if (!animate)
            {
                currentMarker.anchoredPosition = targetAnchoredPosition;
                return;
            }

            markerRoutine = StartCoroutine(MoveMarkerRoutine(targetAnchoredPosition));
        }

        private IEnumerator MoveMarkerRoutine(Vector2 targetPosition)
        {
            if (currentMarker == null)
                yield break;

            Vector2 startPosition = currentMarker.anchoredPosition;
            float timer = 0f;

            while (timer < markerMoveDuration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / markerMoveDuration);
                float eased = EaseOutCubic(t);

                currentMarker.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, eased);

                yield return null;
            }

            currentMarker.anchoredPosition = targetPosition;
        }

        private void PlayZonePulse(ZoneType zoneType)
        {
            if (currentMarker == null)
                return;

            if (pulseRoutine != null)
                StopCoroutine(pulseRoutine);

            pulseRoutine = StartCoroutine(PulseRoutine(zoneType));
        }

        private IEnumerator PulseRoutine(ZoneType zoneType)
        {
            if (currentMarker == null)
                yield break;

            SetMarkerColor(zoneType);

            Vector3 normalScale = Vector3.one;
            Vector3 targetScale = Vector3.one * markerPulseScale;

            float timer = 0f;

            while (timer < markerPulseDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / markerPulseDuration);

                currentMarker.localScale = Vector3.Lerp(normalScale, targetScale, t);
                yield return null;
            }

            timer = 0f;

            while (timer < markerPulseDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / markerPulseDuration);

                currentMarker.localScale = Vector3.Lerp(targetScale, normalScale, t);
                yield return null;
            }

            currentMarker.localScale = normalScale;
            SetMarkerColor(zoneType);
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}