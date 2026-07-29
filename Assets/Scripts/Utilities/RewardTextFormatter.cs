using System.Globalization;

namespace VertigoDemo.WheelOfFortune.Utilities
{
    /// <summary>
    /// Central place for reward number formatting. There is deliberately no
    /// "total reward" formatter any more: totals are per currency, because
    /// adding cash to gold to rifle points produces a meaningless number.
    /// </summary>
    public static class RewardTextFormatter
    {
        public static string FormatAmount(int amount)
        {
            return amount > 0 ? "x" + amount.ToString("N0", CultureInfo.InvariantCulture) : string.Empty;
        }

        public static string FormatAmount(long amount)
        {
            return amount > 0L ? "x" + amount.ToString("N0", CultureInfo.InvariantCulture) : string.Empty;
        }

        public static string FormatBalance(long amount)
        {
            return amount.ToString("N0", CultureInfo.InvariantCulture);
        }

        public static string FormatCost(string currencyName, long cost)
        {
            return cost.ToString("N0", CultureInfo.InvariantCulture) + " " + currencyName.ToUpperInvariant();
        }
    }
}
