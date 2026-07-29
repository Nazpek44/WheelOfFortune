using UnityEngine;

namespace VertigoDemo.WheelOfFortune.UI
{
    /// <summary>
    /// Animation state for a single reward flying from the win popup to the
    /// wallet. Kept as a plain class driven by one shared coroutine rather than
    /// a coroutine per item, so a popup with a dozen rewards still costs one
    /// update loop and there is nothing left running if the popup is destroyed
    /// mid flight.
    /// </summary>
    public sealed class RewardFlight
    {
        private const float POP_SCALE = 1.18f;
        private const float LANDING_SCALE = 0.28f;
        private const float FADE_START = 0.62f;
        private const float SPIN_DEGREES = -22f;

        private readonly RectTransform _rect;
        private readonly CanvasGroup _canvasGroup;
        private readonly Vector3 _start;
        private readonly Vector3 _control;
        private readonly Vector3 _target;
        private readonly float _delay;

        public RewardFlight(
            RectTransform rect,
            CanvasGroup canvasGroup,
            Vector3 start,
            Vector3 target,
            float arcHeight,
            float delay
        )
        {
            _rect = rect;
            _canvasGroup = canvasGroup;
            _start = start;
            _target = target;
            _delay = delay;

            // Control point lifted above the midpoint gives the reward a lob
            // instead of a straight slide, which reads far better at speed.
            Vector3 midpoint = (start + target) * 0.5f;
            _control = new Vector3(midpoint.x, midpoint.y + arcHeight, midpoint.z);
        }

        public void Evaluate(float elapsed, float popDuration, float flightDuration)
        {
            if (!_rect)
                return;

            if (elapsed < popDuration)
            {
                float pop = EaseOutCubic(Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, popDuration)));

                _rect.localScale = Vector3.one * Mathf.Lerp(1f, POP_SCALE, pop);
                return;
            }

            float local = elapsed - popDuration - _delay;

            if (local < 0f)
            {
                _rect.localScale = Vector3.one * POP_SCALE;
                return;
            }

            float progress = Mathf.Clamp01(local / Mathf.Max(0.0001f, flightDuration));

            // Ease in: the reward hangs for a beat, then accelerates away.
            float eased = progress * progress;

            _rect.localPosition = QuadraticBezier(_start, _control, _target, eased);
            _rect.localScale = Vector3.one * Mathf.Lerp(POP_SCALE, LANDING_SCALE, eased);
            _rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, SPIN_DEGREES, progress));

            if (_canvasGroup)
            {
                float fade = Mathf.Clamp01((progress - FADE_START) / (1f - FADE_START));

                _canvasGroup.alpha = 1f - fade;
            }
        }

        public void Finish()
        {
            if (!_rect)
                return;

            _rect.localPosition = _target;
            _rect.localScale = Vector3.one * LANDING_SCALE;

            if (_canvasGroup)
                _canvasGroup.alpha = 0f;
        }

        private static Vector3 QuadraticBezier(Vector3 from, Vector3 control, Vector3 to, float t)
        {
            float inverse = 1f - t;

            return (inverse * inverse * from) + (2f * inverse * t * control) + (t * t * to);
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
