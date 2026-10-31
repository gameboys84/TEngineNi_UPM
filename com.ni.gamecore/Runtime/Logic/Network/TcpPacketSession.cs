using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GameCore
{
    public sealed class TcpPacketSession : IDisposable
    {
        private sealed class Connection
        {
            public readonly TcpClient Client = new TcpClient();
            public readonly CancellationTokenSource Lifetime = new CancellationTokenSource();
            public readonly CancellationToken Token;
            public readonly PacketFramer Framer;
            public long Generation;
            public Connection(int maximumBytes) { Token = Lifetime.Token; Framer = new PacketFramer(maximumBytes); }
            public void Stop() { Lifetime.Cancel(); Client.Close(); }
        }
        private readonly object gate = new object();
        private readonly SemaphoreSlim writes = new SemaphoreSlim(1, 1);
        private readonly ConcurrentQueue<Action> callbacks = new ConcurrentQueue<Action>();
        private readonly int maximumBodyBytes, maximumQueuedPackets, maximumQueuedBodyBytes;
        private Connection connection;
        private int queuedPackets, queuedBodyBytes;
        private long generation;
        private bool disposed;
        public event Action<NetworkPacket> PacketReceived;
        public event Action<Exception> Disconnected;
        public int PendingPackets => Volatile.Read(ref queuedPackets);
        public bool IsConnected { get { lock (gate) return connection != null && connection.Client.Connected; } }
        public TcpPacketSession(int maximumBodyBytes = 8 * 1024 * 1024, int maximumQueuedPackets = 1024, int maximumQueuedBodyBytes = 16 * 1024 * 1024)
        {
            if (maximumBodyBytes < 4 || maximumQueuedPackets < 1 || maximumQueuedBodyBytes < 4) throw new ArgumentOutOfRangeException();
            this.maximumBodyBytes = maximumBodyBytes; this.maximumQueuedPackets = maximumQueuedPackets;
            this.maximumQueuedBodyBytes = maximumQueuedBodyBytes;
        }
        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(host) || port < 1 || port > ushort.MaxValue) throw new ArgumentException("Invalid endpoint.");
            cancellationToken.ThrowIfCancellationRequested();
            var next = new Connection(Math.Min(maximumBodyBytes, maximumQueuedBodyBytes));
            lock (gate)
            {
                if (disposed) { next.Stop(); next.Lifetime.Dispose(); throw new ObjectDisposedException(nameof(TcpPacketSession)); }
                if (connection != null) { next.Stop(); next.Lifetime.Dispose(); throw new InvalidOperationException("Close the current session before reconnecting."); }
                next.Generation = ++generation;
                connection = next;
            }
            try
            {
                using (cancellationToken.Register(() => next.Client.Close())) await next.Client.ConnectAsync(host, port).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                lock (gate) if (connection != next) throw new OperationCanceledException();
                _ = ReceiveAsync(next);
            }
            catch
            {
                lock (gate) if (connection == next) connection = null;
                next.Stop(); next.Lifetime.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }
        public async Task SendAsync(NetworkPacket packet, CancellationToken cancellationToken = default)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));
            Connection current;
            lock (gate) current = connection ?? throw new InvalidOperationException("Session is not connected.");
            byte[] bytes = packet.Encode(maximumBodyBytes);
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(current.Token, cancellationToken))
            {
                await writes.WaitAsync(linked.Token).ConfigureAwait(false);
                try { await current.Client.GetStream().WriteAsync(bytes, 0, bytes.Length, linked.Token).ConfigureAwait(false); }
                catch { CloseConnection(current); throw; }
                finally { writes.Release(); }
            }
        }
        private async Task ReceiveAsync(Connection current)
        {
            Exception failure = null;
            try
            {
                var buffer = new byte[4096];
                NetworkStream stream = current.Client.GetStream();
                while (true)
                {
                    int count = await stream.ReadAsync(buffer, 0, buffer.Length, current.Token).ConfigureAwait(false);
                    if (count == 0) { current.Framer.Complete(); break; }
                    foreach (var packet in current.Framer.Feed(buffer, 0, count))
                    {
                        int bodyBytes = checked(packet.PayloadLength + 4);
                        lock (gate)
                        {
                            if (disposed || current.Token.IsCancellationRequested || generation != current.Generation) break;
                            if (Volatile.Read(ref queuedPackets) >= maximumQueuedPackets || bodyBytes > maximumQueuedBodyBytes - Volatile.Read(ref queuedBodyBytes))
                                throw new IOException("Incoming packet queue is full.");
                            Interlocked.Increment(ref queuedPackets); Interlocked.Add(ref queuedBodyBytes, bodyBytes);
                            callbacks.Enqueue(() =>
                            {
                                Interlocked.Decrement(ref queuedPackets); Interlocked.Add(ref queuedBodyBytes, -bodyBytes);
                                lock (gate) if (disposed || generation != current.Generation) return;
                                PacketReceived?.Invoke(packet);
                            });
                        }
                    }
                }
            }
            catch (Exception error) { if (!current.Token.IsCancellationRequested) failure = error; }
            finally
            {
                bool notify = !current.Token.IsCancellationRequested;
                CloseConnection(current);
                current.Lifetime.Dispose();
                lock (gate) if (notify && !disposed && generation == current.Generation) callbacks.Enqueue(() =>
                {
                    lock (gate) if (disposed || generation != current.Generation) return;
                    Disconnected?.Invoke(failure);
                });
            }
        }
        private void CloseConnection(Connection current)
        {
            lock (gate)
            {
                if (connection != current) return;
                connection = null; current.Stop();
            }
        }
        public int Pump(int maximumCallbacks = 32)
        {
            if (maximumCallbacks < 1) throw new ArgumentOutOfRangeException(nameof(maximumCallbacks));
            int count = 0;
            while (count < maximumCallbacks && callbacks.TryDequeue(out var callback)) { ++count; callback(); }
            return count;
        }
        public void Close() { lock (gate) { ++generation; if (connection != null) CloseConnection(connection); } }
        public void Dispose()
        {
            lock (gate) { if (disposed) return; disposed = true; Close(); }
            while (callbacks.TryDequeue(out var callback)) callback();
        }
    }
}
