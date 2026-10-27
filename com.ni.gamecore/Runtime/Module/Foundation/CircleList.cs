using System;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>Bounded history adapted from DeadEmpire. Returns entries in chronological order.</summary>
    public sealed class CircleList<T>
    {
        private readonly T[] data;
        private int next;
        public int Count { get; private set; }
        public CircleList(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            data = new T[capacity];
        }
        public void Push(T value)
        {
            data[next] = value;
            next = (next + 1) % data.Length;
            Count = Math.Min(Count + 1, data.Length);
        }
        public T[] GetData()
        {
            T[] snapshot = new T[Count];
            int first = Count == data.Length ? next : 0;
            for (int i = 0; i < Count; i++) snapshot[i] = data[(first + i) % data.Length];
            return snapshot;
        }
        public void Clear() { Array.Clear(data, 0, data.Length); Count = 0; next = 0; }
    }
}
