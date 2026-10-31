using System;
using System.Collections.Generic;

namespace GameCore
{
    public sealed class PacketRouter
    {
        private readonly Dictionary<ushort, List<Action<NetworkPacket>>> handlers = new Dictionary<ushort, List<Action<NetworkPacket>>>();
        public IDisposable Subscribe(ushort messageId, Action<NetworkPacket> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (!handlers.TryGetValue(messageId, out var list)) handlers.Add(messageId, list = new List<Action<NetworkPacket>>());
            list.Add(handler);
            return new Subscription(() => { list.Remove(handler); if (list.Count == 0) handlers.Remove(messageId); });
        }
        public int Dispatch(NetworkPacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));
            if (!handlers.TryGetValue(packet.MessageId, out var list)) return 0;
            var snapshot = list.ToArray();
            foreach (var handler in snapshot) handler(packet);
            return snapshot.Length;
        }
        private sealed class Subscription : IDisposable
        {
            private Action release;
            public Subscription(Action release) { this.release = release; }
            public void Dispose() { var action = release; release = null; action?.Invoke(); }
        }
    }
}
