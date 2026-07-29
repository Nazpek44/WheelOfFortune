using System.Collections.Generic;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Persistence boundary for the wallet. The wallet depends on this
    /// abstraction, so swapping PlayerPrefs for a server or a save file is a
    /// one class change that never touches gameplay code.
    /// </summary>
    public interface IWalletStorage
    {
        bool TryLoadCurrencies(Dictionary<CurrencyType, long> destination);

        void SaveCurrencies(IReadOnlyDictionary<CurrencyType, long> balances);

        void Clear();
    }
}
