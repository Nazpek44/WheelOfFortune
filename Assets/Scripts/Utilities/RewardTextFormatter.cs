namespace VertigoDemo.WheelOfFortune.Utilities
{
    public static class RewardTextFormatter
    {
        public static string FormatAmount(int amount)
        {
            return amount > 0 ? $"x{amount}" : string.Empty;
        }

        public static string FormatHudTotal(int total, int itemCount)
        {
            return $"TOTAL: {total} | ITEMS: {itemCount}";
        }

        public static string FormatWinTotal(int total, int itemCount)
        {
            return $"TOTAL REWARD: {total}\nITEMS: {itemCount}";
        }
    }
}