using System;
using System.Collections.Generic;

namespace VertigoDemo.WheelOfFortune.Economy
{
    public sealed class PlayerWallet : IPlayerWallet
    {
        private readonly Dictionary<CurrencyType, long> _balances = new Dictionary<CurrencyType, long>();
        private readonly Dictionary<string, RewardEntry> _itemsById = new Dictionary<string, RewardEntry>();
        private readonly List<RewardEntry> _items = new List<RewardEntry>();
        private readonly IWalletStorage _storage;
        private readonly IReadOnlyDictionary<CurrencyType, long> _startingBalances;

        public event Action Changed;

        public IReadOnlyList<RewardEntry> Items => _items;

        public PlayerWallet(IWalletStorage storage, IReadOnlyDictionary<CurrencyType, long> startingBalances)
        {
            _storage = storage;
            _startingBalances = startingBalances ?? new Dictionary<CurrencyType, long>();

            if (_storage == null || !_storage.TryLoadCurrencies(_balances))
                ApplyStartingBalances();
        }

        public long GetBalance(CurrencyType currency)
        {
            return _balances.TryGetValue(currency, out long balance) ? balance : 0L;
        }

        public bool CanAfford(CurrencyType currency, long amount)
        {
            if (currency == CurrencyType.None)
                return false;

            if (amount <= 0L)
                return true;

            return GetBalance(currency) >= amount;
        }

        public bool TrySpend(CurrencyType currency, long amount)
        {
            if (!CanAfford(currency, amount))
                return false;

            if (amount > 0L)
            {
                _balances[currency] = GetBalance(currency) - amount;
                Save();
                RaiseChanged();
            }

            return true;
        }

        public void AddCurrency(CurrencyType currency, long amount)
        {
            if (currency == CurrencyType.None || amount == 0L)
                return;

            _balances[currency] = GetBalance(currency) + amount;

            Save();
            RaiseChanged();
        }

        public void Deposit(IReadOnlyList<RewardEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                RewardEntry entry = entries[i];

                if (entry == null)
                    continue;

                if (entry.IsCurrency)
                {
                    _balances[entry.Currency] = GetBalance(entry.Currency) + entry.TotalAmount;
                    continue;
                }

                AddItemInternal(entry);
            }

            Save();
            RaiseChanged();
        }

        public void ResetToStartingBalances()
        {
            _balances.Clear();
            _itemsById.Clear();
            _items.Clear();

            ApplyStartingBalances();

            _storage?.Clear();

            Save();
            RaiseChanged();
        }

        private void AddItemInternal(RewardEntry entry)
        {
            if (_itemsById.TryGetValue(entry.RewardId, out RewardEntry existing))
            {
                existing.AddAmount(entry.TotalAmount);
                return;
            }

            RewardEntry stored = new RewardEntry(
                entry.RewardId,
                entry.DisplayName,
                entry.Icon,
                entry.Kind,
                entry.Currency,
                entry.TotalAmount,
                entry.Tier
            );

            _itemsById.Add(stored.RewardId, stored);
            _items.Add(stored);
        }

        private void ApplyStartingBalances()
        {
            foreach (KeyValuePair<CurrencyType, long> pair in _startingBalances)
                _balances[pair.Key] = pair.Value;
        }

        private void Save()
        {
            _storage?.SaveCurrencies(_balances);
        }

        private void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}
