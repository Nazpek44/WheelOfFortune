using System.Collections;
using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Animation
{
    public sealed class CoroutineWheelSpinner : MonoBehaviour, IWheelSpinner
    {
        public bool IsSpinning { get; private set; }

        public IEnumerator Spin(
            RectTransform rotator,
            int sliceCount,
            int resultIndex,
            float duration,
            int fullRotations
        )
        {
            if (rotator == null || sliceCount <= 0)
                yield break;

            IsSpinning = true;

            float sliceAngle = 360f / sliceCount;

            float startAngle = rotator.localEulerAngles.z;
            float targetAngle = resultIndex * sliceAngle;

            float deltaToTarget = Mathf.Repeat(targetAngle - startAngle, 360f);
            float totalRotation = fullRotations * 360f + deltaToTarget;

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / duration);
                float eased = EaseOutCubic(t);

                float currentAngle = startAngle + totalRotation * eased;

                rotator.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

                yield return null;
            }

            rotator.localRotation = Quaternion.Euler(0f, 0f, targetAngle);

            IsSpinning = false;
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}