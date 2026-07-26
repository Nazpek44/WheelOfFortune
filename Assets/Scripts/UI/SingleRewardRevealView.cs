using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class SingleRewardRevealView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image flashImage;
        [SerializeField] private Image rewardIconImage;
        [SerializeField] private TMP_Text rewardNameText_value;
        [SerializeField] private TMP_Text rewardAmountText_value;

        [Header("Animation Settings")]
        [SerializeField] private float appearDuration = 0.25f;
        [SerializeField] private float stayDuration = 0.65f;
        [SerializeField] private float disappearDuration = 0.25f;
        [SerializeField] private float flashRotationSpeed = 220f;

        private Coroutine revealRoutine;

        private void Awake()
        {
            CacheReferences();
            HideInstant();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (flashImage == null)
                flashImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_single_reward_flash");

            if (rewardIconImage == null)
                rewardIconImage = ComponentFinder.FindChildByName<Image>(this, "ui_image_single_reward_icon");

            if (rewardNameText_value == null)
                rewardNameText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_single_reward_name_value");

            if (rewardAmountText_value == null)
                rewardAmountText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_single_reward_amount_value");
        }

        public void Show(WheelSlice reward)
        {
            if (reward == null)
            {
                Debug.LogWarning("SingleRewardRevealView.Show failed because reward is null.");
                return;
            }

            if (reward.isBomb)
                return;

            CacheReferences();

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            SetRewardVisuals(reward);

            if (revealRoutine != null)
                StopCoroutine(revealRoutine);

            revealRoutine = StartCoroutine(RevealRoutine());
        }

        private void SetRewardVisuals(WheelSlice reward)
        {
            if (rewardIconImage != null)
            {
                rewardIconImage.sprite = reward.icon;
                rewardIconImage.enabled = reward.icon != null;
                rewardIconImage.preserveAspect = true;
                rewardIconImage.raycastTarget = false;
            }

            if (rewardNameText_value != null)
            {
                rewardNameText_value.text = reward.rewardName;
                rewardNameText_value.raycastTarget = false;
            }

            if (rewardAmountText_value != null)
            {
                rewardAmountText_value.text = reward.GetDisplayText();
                rewardAmountText_value.raycastTarget = false;
            }

            if (flashImage != null)
            {
                flashImage.enabled = true;
                flashImage.raycastTarget = false;
            }
        }

        private IEnumerator RevealRoutine()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            transform.localScale = Vector3.one * 0.35f;

            float timer = 0f;

            while (timer < appearDuration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / appearDuration);
                float eased = EaseOutBack(t);

                transform.localScale = Vector3.LerpUnclamped(
                    Vector3.one * 0.35f,
                    Vector3.one * 1.12f,
                    eased
                );

                canvasGroup.alpha = t;

                RotateFlash();

                yield return null;
            }

            transform.localScale = Vector3.one;
            canvasGroup.alpha = 1f;

            timer = 0f;

            while (timer < stayDuration)
            {
                timer += Time.deltaTime;
                RotateFlash();
                yield return null;
            }

            timer = 0f;

            while (timer < disappearDuration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / disappearDuration);

                transform.localScale = Vector3.Lerp(
                    Vector3.one,
                    Vector3.one * 0.65f,
                    t
                );

                canvasGroup.alpha = 1f - t;

                RotateFlash();

                yield return null;
            }

            HideInstant();
        }

        private void RotateFlash()
        {
            if (flashImage != null)
                flashImage.transform.Rotate(0f, 0f, -flashRotationSpeed * Time.deltaTime);
        }

        private void HideInstant()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}