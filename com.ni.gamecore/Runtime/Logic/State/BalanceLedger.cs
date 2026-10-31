using System;
using System.Collections.Generic;

namespace GameCore
{
    public enum LedgerResult { Applied, Duplicate, InsufficientBalance }
    [Serializable] public sealed class BalanceEntry { public string Key; public long Value; }
    [Serializable] public sealed class BalanceSnapshot
    {
        public BalanceEntry[] Entries = Array.Empty<BalanceEntry>();
        public string[] Receipts = Array.Empty<string>();
    }
    public sealed class BalanceLedger
    {
        private Dictionary<string, long> balances = new Dictionary<string, long>(StringComparer.Ordinal);
        private HashSet<string> receipts = new HashSet<string>(StringComparer.Ordinal);
        public long Get(string key) => balances.TryGetValue(key, out long value) ? value : 0;
        public bool ContainsReceipt(string id) => receipts.Contains(id);
        public LedgerResult TryApply(string transactionId, IReadOnlyDictionary<string, long> changes)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("A transaction id is required.");
            if (changes == null) throw new ArgumentNullException(nameof(changes));
            if (receipts.Contains(transactionId)) return LedgerResult.Duplicate;
            var proposed = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (var pair in changes)
            {
                if (string.IsNullOrWhiteSpace(pair.Key)) throw new ArgumentException("A balance key is required.");
                long value = checked(Get(pair.Key) + pair.Value);
                if (value < 0) return LedgerResult.InsufficientBalance;
                proposed.Add(pair.Key, value);
            }
            foreach (var pair in proposed) balances[pair.Key] = pair.Value;
            receipts.Add(transactionId);
            return LedgerResult.Applied;
        }
        public BalanceSnapshot Export()
        {
            var keys = new List<string>(balances.Keys); keys.Sort(StringComparer.Ordinal);
            var entries = new BalanceEntry[keys.Count];
            for (int i = 0; i < keys.Count; i++) entries[i] = new BalanceEntry { Key = keys[i], Value = balances[keys[i]] };
            var ids = new string[receipts.Count]; receipts.CopyTo(ids); Array.Sort(ids, StringComparer.Ordinal);
            return new BalanceSnapshot { Entries = entries, Receipts = ids };
        }
        public void Restore(BalanceSnapshot snapshot)
        {
            if (snapshot?.Entries == null || snapshot.Receipts == null) throw new ArgumentException("Invalid balance snapshot.");
            var values = new Dictionary<string, long>(StringComparer.Ordinal);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in snapshot.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key) || entry.Value < 0 || values.ContainsKey(entry.Key))
                    throw new ArgumentException("Invalid balance entry.");
                values.Add(entry.Key, entry.Value);
            }
            foreach (string id in snapshot.Receipts)
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id)) throw new ArgumentException("Invalid transaction receipt.");
            balances = values; receipts = ids;
        }
    }
}
