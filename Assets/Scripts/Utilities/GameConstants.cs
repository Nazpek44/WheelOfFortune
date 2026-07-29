namespace VertigoDemo.WheelOfFortune.Utilities
{
    public static class GameConstants
    {
        public const int FIRST_ZONE = 1;
        public const int SAFE_ZONE_INTERVAL = 5;
        public const int SUPER_ZONE_INTERVAL = 30;

        public const float DEFAULT_SPIN_DURATION = 3f;
        public const int DEFAULT_FULL_ROTATIONS = 6;

        public const int VISIBLE_PROGRESS_STEP_COUNT = 9;

        /// <summary>Distance from the wheel centre to each reward slot.</summary>
        public const float WHEEL_SLOT_RADIUS = 305f;

        /// <summary>Angle of the fixed indicator, measured counter clockwise from +X.</summary>
        public const float WHEEL_INDICATOR_ANGLE = 90f;
    }
}
