using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Demo grade persistence. One key per currency keeps the save readable and
    /// means a newly added <see cref="CurrencyType"/> needs no migration.
    /// </summary>
    public sealed class PlayerPrefsWalletStorage : IWalletStorage
    {
        private const string KEY_PREFIX = "wallet.currency.";
        private const string SAVE_FLAG_KEY = "wallet.saved";

        public bool TryLoadCurrencies(Dictionary<CurrencyType, long> destination)
        {
            if (destination == null)
                return false;

            if (PlayerPrefs.GetInt(SAVE_FLAG_KEY, 0) == 0)
                return false;

            destination.Clear();

            foreach (CurrencyType currency in AllCurrencies())
            {
                string raw = PlayerPrefs.GetString(KEY_PREFIX + currency, string.Empty);

                if (string.IsNullOrEmpty(raw))
                    continue;

                if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
                    destination[currency] = value;
            }

            return true;
        }

        public void SaveCurrencies(IReadOnlyDictionary<CurrencyType, long> balances)
        {
            if (balances == null)
                return;

            foreach (KeyValuePair<CurrencyType, long> pair in balances)
            {
                PlayerPrefs.SetString(
                    KEY_PREFIX + pair.Key,
                    pair.Value.ToString(CultureInfo.InvariantCulture)
                );
            }

            PlayerPrefs.SetInt(SAVE_FLAG_KEY, 1);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            foreach (CurrencyType currency in AllCurrencies())
                PlayerPrefs.DeleteKey(KEY_PREFIX + currency);

            PlayerPrefs.DeleteKey(SAVE_FLAG_KEY);
            PlayerPrefs.Save();
        }

        private static IEnumerable<CurrencyType> AllCurrencies()
        {
            foreach (CurrencyType currency in (CurrencyType[])Enum.GetValues(typeof(CurrencyType)))
            {
                if (currency != CurrencyType.None)
                    yield return currency;
            }
        }
    }
}
