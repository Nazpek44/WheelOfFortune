using System.Collections.Generic;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Rewards collected during the current run only. Nothing here belongs to
    /// the player yet: the contents are banked into <see cref="IPlayerWallet"/>
    /// when the player leaves on a safe zone, and wiped when a bomb resolves.
    /// </summary>
    public interface IRunRewardInventory
    {
        IReadOnlyList<RewardEntry> Entries { get; }

        /// <summary>Number of distinct grouped entries.</summary>
        int EntryCount { get; }

        /// <summary>Number of individual rewards collected this run.</summary>
        int CollectedCount { get; }

        bool IsEmpty { get; }

        long GetCurrencyTotal(CurrencyType currency);

        void Add(RewardDraw reward);

        void Clear();
    }
}
