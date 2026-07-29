using System.Collections;
using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Animation
{
    public sealed class CoroutineWheelSpinnerMono : MonoBehaviour, IWheelSpinner
    {
        [Tooltip("Guards against a zero duration producing a division by zero.")]
        [Min(0.05f)]
        [SerializeField] private float _minimumDuration = 0.05f;

        public bool IsSpinning { get; private set; }

        public IEnumerator SpinCoroutine(
            RectTransform rotator,
            int sliceCount,
            int resultIndex,
            float duration,
            int fullRotations
        )
        {
            if (!rotator || sliceCount <= 0)
                yield break;

            IsSpinning = true;

            // A zero or negative duration used to produce NaN angles and freeze
            // the wheel; clamping here keeps a bad inspector value harmless.
            float safeDuration = Mathf.Max(_minimumDuration, duration);
            int safeRotations = Mathf.Max(0, fullRotations);

            float sliceAngle = 360f / sliceCount;
            float startAngle = rotator.localEulerAngles.z;
            float targetAngle = resultIndex * sliceAngle;

            float deltaToTarget = Mathf.Repeat(targetAngle - startAngle, 360f);
            float totalRotation = safeRotations * 360f + deltaToTarget;

            float timer = 0f;

            while (timer < safeDuration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / safeDuration);
                float eased = EaseOutCubic(progress);
                float currentAngle = startAngle + totalRotation * eased;

                rotator.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

                yield return null;
            }

            rotator.localRotation = Quaternion.Euler(0f, 0f, targetAngle);

            IsSpinning = false;
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
