using System.Collections.Generic;

namespace VertigoDemo.WheelOfFortune.Economy
{
    /// <summary>
    /// Groups the run's rewards by id and keeps a separate running total per
    /// currency. There is deliberately no single "total reward" number, because
    /// adding cash to gold to rifle points produces a figure that means nothing.
    /// </summary>
    public sealed class RunRewardInventory : IRunRewardInventory
    {
        private readonly Dictionary<string, RewardEntry> _entriesById = new Dictionary<string, RewardEntry>();
        private readonly List<RewardEntry> _entries = new List<RewardEntry>();
        private readonly Dictionary<CurrencyType, long> _currencyTotals = new Dictionary<CurrencyType, long>();

        public IReadOnlyList<RewardEntry> Entries => _entries;
        public int EntryCount => _entries.Count;
        public int CollectedCount { get; private set; }
        public bool IsEmpty => _entries.Count == 0;

        public long GetCurrencyTotal(CurrencyType currency)
        {
            return _currencyTotals.TryGetValue(currency, out long total) ? total : 0L;
        }

        public void Add(RewardDraw reward)
        {
            if (reward.IsBomb)
                return;

            int amount = reward.Amount > 0 ? reward.Amount : 1;

            CollectedCount++;

            if (reward.IsCurrency)
            {
                _currencyTotals.TryGetValue(reward.Currency, out long currentTotal);
                _currencyTotals[reward.Currency] = currentTotal + amount;
            }

            string key = BuildKey(reward);

            if (_entriesById.TryGetValue(key, out RewardEntry existingEntry))
            {
                existingEntry.AddAmount(amount);
                return;
            }

            RewardEntry newEntry = new RewardEntry(
                key,
                reward.DisplayName,
                reward.Icon,
                reward.Kind,
                reward.Currency,
                amount,
                reward.Tier
            );

            _entriesById.Add(key, newEntry);
            _entries.Add(newEntry);
        }

        public void Clear()
        {
            _entriesById.Clear();
            _entries.Clear();
            _currencyTotals.Clear();

            CollectedCount = 0;
        }

        /// <summary>
        /// Currencies group by currency type so that "Cash" won from a bronze
        /// wheel and "Cash " won from a silver wheel land in the same stack.
        /// </summary>
        private static string BuildKey(RewardDraw reward)
        {
            if (reward.IsCurrency)
                return "currency_" + reward.Currency;

            string id = string.IsNullOrWhiteSpace(reward.RewardId) ? "reward_unknown" : reward.RewardId;

            // Tier is part of the identity: a Tier 1 and a Tier 3 gift are
            // different rewards even when they share a family name.
            return reward.Tier > 0 ? id + "_t" + reward.Tier : id;
        }
    }
}
