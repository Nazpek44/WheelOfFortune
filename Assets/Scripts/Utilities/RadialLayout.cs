using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Utilities
{
    /// <summary>
    /// Places wheel slots on an exact circle around the rotator pivot.
    /// Hand positioned slots were the real cause of the wheel appearing to spin
    /// off centre: each icon orbited on a slightly different radius, so the ring
    /// visibly wobbled even though the rotator pivot was already centred.
    /// </summary>
    public static class RadialLayout
    {
        /// <summary>
        /// Local position of slot <paramref name="index"/> of
        /// <paramref name="slotCount"/>, laid out clockwise starting at the
        /// indicator angle.
        /// </summary>
        public static Vector2 GetSlotPosition(int index, int slotCount, float radius)
        {
            if (slotCount <= 0)
                return Vector2.zero;

            float angleDegrees = GetSlotAngle(index, slotCount);
            float angleRadians = angleDegrees * Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(angleRadians) * radius,
                Mathf.Sin(angleRadians) * radius
            );
        }

        /// <summary>Angle of slot <paramref name="index"/>, counter clockwise from +X.</summary>
        public static float GetSlotAngle(int index, int slotCount)
        {
            if (slotCount <= 0)
                return GameConstants.WHEEL_INDICATOR_ANGLE;

            float step = 360f / slotCount;

            return GameConstants.WHEEL_INDICATOR_ANGLE - step * index;
        }

        /// <summary>Z rotation that keeps a slot upright relative to the wheel.</summary>
        public static float GetSlotRotation(int index, int slotCount)
        {
            if (slotCount <= 0)
                return 0f;

            return -(360f / slotCount) * index;
        }
    }
}
