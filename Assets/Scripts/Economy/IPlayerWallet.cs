using System;
using System.Collections.Generic;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// The player's real, persistent inventory. Balances survive restarts, are
    /// credited when a run is banked, and are debited when the player pays to
    /// continue after a bomb.
    /// </summary>
    public interface IPlayerWallet
    {
        event Action Changed;

        IReadOnlyList<RewardEntry> Items { get; }

        long GetBalance(CurrencyType currency);

        bool CanAfford(CurrencyType currency, long amount);

        /// <summary>Debits the balance. Returns false and changes nothing when the player cannot afford it.</summary>
        bool TrySpend(CurrencyType currency, long amount);

        void AddCurrency(CurrencyType currency, long amount);

        /// <summary>Credits every entry of a finished run into the wallet.</summary>
        void Deposit(IReadOnlyList<RewardEntry> entries);

        void ResetToStartingBalances();
    }
}
