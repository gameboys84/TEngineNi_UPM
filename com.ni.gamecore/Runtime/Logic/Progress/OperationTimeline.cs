using System;
using System.Collections.Generic;

namespace GameCore
{
    [Serializable] public sealed class TimedOperation
    {
        public string Id, Group;
        public int EntityId;
        public long StartMilliseconds, EndMilliseconds;
        public TimedOperation Copy() => (TimedOperation)MemberwiseClone();
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(Group) || EntityId <= 0 ||
                StartMilliseconds < 0 || EndMilliseconds < StartMilliseconds) throw new ArgumentException("Invalid timed operation.");
        }
        public long Remaining(long now) => Math.Max(0, checked(EndMilliseconds - now));
        public double Fraction(long now)
        {
            if (now < StartMilliseconds) return 0;
            if (EndMilliseconds == StartMilliseconds) return 1;
            return Math.Min(1, Math.Max(0, (now - (double)StartMilliseconds) / (EndMilliseconds - (double)StartMilliseconds)));
        }
    }
    public sealed class OperationTimeline
    {
        private readonly Dictionary<string, TimedOperation> entries = new Dictionary<string, TimedOperation>(StringComparer.Ordinal);
        private readonly Dictionary<string, long> revisions = new Dictionary<string, long>(StringComparer.Ordinal);
        public int Count => entries.Count;
        public TimedOperation Get(string id) => entries.TryGetValue(id, out var value) ? value.Copy() : null;
        public TimedOperation[] GetAll()
        {
            var result = new List<TimedOperation>();
            foreach (var value in entries.Values) result.Add(value.Copy());
            result.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            return result.ToArray();
        }
        public bool ApplySnapshot(string group, long revision, IEnumerable<TimedOperation> values)
        {
            if (string.IsNullOrWhiteSpace(group) || revision < 0 || values == null) throw new ArgumentException("Invalid operation snapshot.");
            if (revisions.TryGetValue(group, out long current) && revision <= current) return false;
            var replacement = new Dictionary<string, TimedOperation>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (value == null) throw new ArgumentException("Null operation.");
                value.Validate();
                if (value.Group != group || replacement.ContainsKey(value.Id) ||
                    (entries.TryGetValue(value.Id, out var existing) && existing.Group != group))
                    throw new ArgumentException("Duplicate or mismatched operation identity.");
                replacement.Add(value.Id, value.Copy());
            }
            var removed = new List<string>();
            foreach (var pair in entries) if (pair.Value.Group == group) removed.Add(pair.Key);
            foreach (string id in removed) entries.Remove(id);
            foreach (var pair in replacement) entries.Add(pair.Key, pair.Value);
            revisions[group] = revision;
            return true;
        }
    }
}
