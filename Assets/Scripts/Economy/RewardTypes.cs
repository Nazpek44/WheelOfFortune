namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Distinguishes spendable currencies from non-spendable collectibles.
    /// Item is the default so that any wheel slice authored before the economy
    /// layer existed is treated as a plain collectible instead of free money.
    /// </summary>
    public enum RewardKind
    {
        Item = 0,
        Currency = 1
    }

    /// <summary>
    /// Every spendable currency in the game. Adding a new one here is the only
    /// change required: the wallet, the inventory and the HUD are all keyed by
    /// this enum rather than by a hard coded field per currency.
    /// </summary>
    public enum CurrencyType
    {
        None = 0,
        Cash = 1,
        Gold = 2
    }
}
