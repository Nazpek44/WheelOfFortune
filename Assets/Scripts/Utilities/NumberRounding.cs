using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Utilities
{
    /// <summary>
    /// Rounding helpers for scaled reward amounts. Raw curve output such as
    /// 1284.37 reads badly on a wheel slice, so amounts are snapped to two
    /// significant digits once they get large.
    /// </summary>
    public static class NumberRounding
    {
        private const int SIGNIFICANT_DIGITS = 2;
        private const float SMALL_AMOUNT_THRESHOLD = 100f;

        /// <summary>
        /// Rounds a scaled amount to a value a player can read at a glance.
        /// Never returns less than one, so a reward can never scale to nothing.
        /// </summary>
        public static int ToReadableAmount(float value)
        {
            if (value <= 1f)
                return 1;

            if (value < SMALL_AMOUNT_THRESHOLD)
                return Mathf.Max(1, Mathf.RoundToInt(value));

            int digits = Mathf.FloorToInt(Mathf.Log10(value)) + 1;
            int step = Mathf.RoundToInt(Mathf.Pow(10f, Mathf.Max(0, digits - SIGNIFICANT_DIGITS)));

            if (step <= 1)
                return Mathf.Max(1, Mathf.RoundToInt(value));

            int rounded = Mathf.RoundToInt(value / step) * step;

            return Mathf.Max(step, rounded);
        }
    }
}
