using System;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>Max heap adapted from DeadEmpire PriorityQueue. No Unity or game dependencies.</summary>
    public sealed class PriorityQueue<T>
    {
        private readonly IComparer<T> comparer;
        private T[] heap;
        public int Count { get; private set; }

        public PriorityQueue() : this(16, null) { }
        public PriorityQueue(int capacity) : this(capacity, null) { }
        public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }
        public PriorityQueue(int capacity, IComparer<T> comparer)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            heap = new T[Math.Max(1, capacity)];
            this.comparer = comparer ?? Comparer<T>.Default;
        }
        public void Clear() { Array.Clear(heap, 0, heap.Length); Count = 0; }
        public void Push(T value)
        {
            if (Count == heap.Length) Array.Resize(ref heap, checked(heap.Length * 2));
            int index = Count++;
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (comparer.Compare(value, heap[parent]) <= 0) break;
                heap[index] = heap[parent];
                index = parent;
            }
            heap[index] = value;
        }
        public T Top()
        {
            if (Count == 0) throw new InvalidOperationException("Queue is empty.");
            return heap[0];
        }
        public T Pop()
        {
            T result = Top();
            T value = heap[--Count];
            heap[Count] = default;
            if (Count == 0) return result;
            int index = 0;
            while (index < Count / 2)
            {
                int child = index * 2 + 1;
                if (child + 1 < Count && comparer.Compare(heap[child + 1], heap[child]) > 0) child++;
                if (comparer.Compare(value, heap[child]) >= 0) break;
                heap[index] = heap[child];
                index = child;
            }
            heap[index] = value;
            return result;
        }
    }
}
