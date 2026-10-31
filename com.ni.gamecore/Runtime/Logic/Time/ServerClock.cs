using System;
using System.Diagnostics;

namespace GameCore
{
    public sealed class ServerClock
    {
        private readonly object gate = new object();
        private readonly Func<long> monotonicMilliseconds;
        private long anchorLocal, anchorServer, lastReturned;
        private bool synchronized;

        public ServerClock(Func<long> monotonicMilliseconds = null)
        {
            var stopwatch = Stopwatch.StartNew();
            this.monotonicMilliseconds = monotonicMilliseconds ?? (() => stopwatch.ElapsedMilliseconds);
        }
        public bool IsSynchronized { get { lock (gate) return synchronized; } }
        public void Synchronize(long serverMilliseconds, long roundTripMilliseconds = 0)
        {
            if (serverMilliseconds < 0 || roundTripMilliseconds < 0) throw new ArgumentOutOfRangeException();
            lock (gate)
            {
                long corrected = checked(serverMilliseconds + roundTripMilliseconds / 2);
                long local = monotonicMilliseconds();
                if (local < 0) throw new InvalidOperationException("Monotonic time must be nonnegative.");
                anchorLocal = local;
                anchorServer = Math.Max(lastReturned, corrected);
                synchronized = true;
            }
        }
        public long NowMilliseconds
        {
            get
            {
                lock (gate)
                {
                    if (!synchronized) throw new InvalidOperationException("Server time has not been synchronized.");
                    long elapsed = Math.Max(0, checked(monotonicMilliseconds() - anchorLocal));
                    lastReturned = Math.Max(lastReturned, checked(anchorServer + elapsed));
                    return lastReturned;
                }
            }
        }
        public void Reset()
        {
            lock (gate) { synchronized = false; anchorLocal = anchorServer = lastReturned = 0; }
        }
    }
}
