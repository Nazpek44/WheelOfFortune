using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class SingleRewardRevealViewMono : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _flashImage;
        [SerializeField] private Image _rewardIconImage;
        [SerializeField] private TMP_Text _rewardNameText;
        [SerializeField] private TMP_Text _rewardAmountText;

        [Header("Animation Settings")]
        [Range(0.05f, 1f)]
        [SerializeField] private float _appearDuration = 0.25f;

        [Range(0.05f, 3f)]
        [SerializeField] private float _stayDuration = 0.65f;

        [Range(0.05f, 1f)]
        [SerializeField] private float _disappearDuration = 0.25f;

        [Range(0f, 720f)]
        [SerializeField] private float _flashRotationSpeed = 220f;

        private Coroutine _revealCoroutine;

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _flashImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_single_reward_flash");
            _rewardIconImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_single_reward_icon");
            _rewardNameText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_single_reward_name_value");
            _rewardAmountText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_single_reward_amount_value");
        }

        private void Awake()
        {
            if (!_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (!_canvasGroup)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            HideInstant();
        }

        private void OnDestroy()
        {
            StopReveal();
        }

        public void Show(RewardDraw draw)
        {
            if (draw.IsBomb)
                return;

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            SetRewardVisuals(draw);

            StopReveal();

            _revealCoroutine = StartCoroutine(RevealCoroutine());
        }

        private void SetRewardVisuals(RewardDraw draw)
        {
            if (_rewardIconImage)
            {
                _rewardIconImage.sprite = draw.Icon;
                _rewardIconImage.enabled = draw.Icon;
                _rewardIconImage.preserveAspect = true;
                _rewardIconImage.raycastTarget = false;
            }

            if (_rewardNameText)
            {
                _rewardNameText.text = draw.DisplayName;
                _rewardNameText.raycastTarget = false;
            }

            if (_rewardAmountText)
            {
                _rewardAmountText.text = RewardTextFormatter.FormatAmount(draw.Amount);
                _rewardAmountText.raycastTarget = false;
            }

            if (_flashImage)
            {
                _flashImage.enabled = true;
                _flashImage.raycastTarget = false;
            }
        }

        private IEnumerator RevealCoroutine()
        {
            if (!_canvasGroup)
                yield break;

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            Vector3 smallScale = Vector3.one * 0.35f;
            Vector3 overshootScale = Vector3.one * 1.12f;

            transform.localScale = smallScale;

            float timer = 0f;

            while (timer < _appearDuration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / _appearDuration);

                transform.localScale = Vector3.LerpUnclamped(smallScale, overshootScale, EaseOutBack(progress));
                _canvasGroup.alpha = progress;

                RotateFlash();

                yield return null;
            }

            transform.localScale = Vector3.one;
            _canvasGroup.alpha = 1f;

            timer = 0f;

            while (timer < _stayDuration)
            {
                timer += Time.deltaTime;
                RotateFlash();
                yield return null;
            }

            timer = 0f;

            while (timer < _disappearDuration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / _disappearDuration);

                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.65f, progress);
                _canvasGroup.alpha = 1f - progress;

                RotateFlash();

                yield return null;
            }

            _revealCoroutine = null;

            HideInstant();
        }

        private void RotateFlash()
        {
            if (_flashImage)
                _flashImage.transform.Rotate(0f, 0f, -_flashRotationSpeed * Time.deltaTime);
        }

        private void StopReveal()
        {
            if (_revealCoroutine != null)
                StopCoroutine(_revealCoroutine);

            _revealCoroutine = null;
        }

        private void HideInstant()
        {
            if (_canvasGroup)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }

        private static float EaseOutBack(float t)
        {
            const float C1 = 1.70158f;
            const float C3 = C1 + 1f;

            return 1f + C3 * Mathf.Pow(t - 1f, 3f) + C1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
