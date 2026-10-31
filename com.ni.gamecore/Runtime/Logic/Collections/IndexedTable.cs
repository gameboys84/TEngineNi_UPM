using System;
using System.Collections.Generic;

namespace GameCore
{
    public sealed class IndexedTable<T>
    {
        private readonly Func<T, int> key;
        private Dictionary<int, T> entries = new Dictionary<int, T>();
        public IndexedTable(Func<T, int> key) { this.key = key ?? throw new ArgumentNullException(nameof(key)); }
        public int Count => entries.Count;
        public T Get(int id) => entries[id];
        public bool TryGet(int id, out T value) => entries.TryGetValue(id, out value);
        public T[] GetAll() { var copy = new T[entries.Count]; entries.Values.CopyTo(copy, 0); return copy; }
        public void Replace(IEnumerable<T> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var replacement = new Dictionary<int, T>();
            foreach (T value in values)
            {
                if (ReferenceEquals(value, null)) throw new ArgumentException("Null table entry.");
                int id = key(value);
                if (id <= 0 || replacement.ContainsKey(id)) throw new ArgumentException("Invalid or duplicate table key.");
                replacement.Add(id, value);
            }
            entries = replacement;
        }
    }
}
