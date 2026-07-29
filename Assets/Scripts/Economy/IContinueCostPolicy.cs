namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Prices the optional "continue" bonus that lets a player keep a run alive
    /// after a bomb. Continuing is never free: it is charged against the
    /// player's persistent wallet.
    /// </summary>
    public interface IContinueCostPolicy
    {
        CurrencyType Currency { get; }

        long GetCost(int zone, int continuesUsedThisRun);
    }
}
