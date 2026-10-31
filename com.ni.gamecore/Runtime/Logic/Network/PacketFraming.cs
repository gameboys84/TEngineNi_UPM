using System;
using System.Collections.Generic;
using System.IO;

namespace GameCore
{
    public sealed class NetworkPacket
    {
        public ushort NodeId { get; }
        public ushort MessageId { get; }
        private readonly byte[] payload;
        public int PayloadLength => payload.Length;
        public byte[] GetPayload() => (byte[])payload.Clone();
        public NetworkPacket(ushort messageId, byte[] payload, ushort nodeId = 0)
        {
            MessageId = messageId; NodeId = nodeId;
            this.payload = (byte[])(payload ?? throw new ArgumentNullException(nameof(payload))).Clone();
        }
        public byte[] Encode(int maximumBodyBytes = 8 * 1024 * 1024)
        {
            int length = checked(payload.Length + 4);
            if (length > maximumBodyBytes) throw new InvalidDataException("Packet exceeds the configured limit.");
            var writer = new PacketWriter();
            if (length < ushort.MaxValue) writer.WriteUInt16((ushort)length);
            else { writer.WriteUInt16(ushort.MaxValue); writer.WriteUInt32((uint)length); }
            writer.WriteUInt16(NodeId); writer.WriteUInt16(MessageId); writer.WriteRaw(payload);
            return writer.ToArray();
        }
    }
    public sealed class PacketFramer
    {
        private readonly int maximumBodyBytes;
        private readonly byte[] header = new byte[6];
        private int headerCount, headerLength = 2, bodyCount;
        private byte[] body;
        private bool faulted;
        public PacketFramer(int maximumBodyBytes = 8 * 1024 * 1024)
        {
            if (maximumBodyBytes < 4) throw new ArgumentOutOfRangeException(nameof(maximumBodyBytes));
            this.maximumBodyBytes = maximumBodyBytes;
        }
        public NetworkPacket[] Feed(byte[] input, int offset, int count)
        {
            if (input == null || offset < 0 || count < 0 || offset > input.Length - count) throw new ArgumentException("Invalid input range.");
            if (faulted) throw new InvalidOperationException("Reset the decoder after malformed input.");
            var packets = new List<NetworkPacket>();
            try
            {
                while (count > 0)
                {
                    if (body == null)
                    {
                        int take = Math.Min(count, headerLength - headerCount);
                        Buffer.BlockCopy(input, offset, header, headerCount, take);
                        headerCount += take; offset += take; count -= take;
                        if (headerCount < headerLength) continue;
                        int shortLength = (header[0] << 8) | header[1];
                        if (shortLength == ushort.MaxValue && headerLength == 2) { headerLength = 6; continue; }
                        uint length = shortLength == ushort.MaxValue
                            ? ((uint)header[2] << 24) | ((uint)header[3] << 16) | ((uint)header[4] << 8) | header[5]
                            : (uint)shortLength;
                        if (length < 4 || length > maximumBodyBytes) throw new InvalidDataException("Invalid packet body length.");
                        body = new byte[(int)length];
                    }
                    int copied = Math.Min(count, body.Length - bodyCount);
                    Buffer.BlockCopy(input, offset, body, bodyCount, copied);
                    bodyCount += copied; offset += copied; count -= copied;
                    if (bodyCount != body.Length) continue;
                    var reader = new PacketReader(body);
                    ushort node = reader.ReadUInt16(), message = reader.ReadUInt16();
                    packets.Add(new NetworkPacket(message, reader.ReadRaw(reader.Remaining), node));
                    ClearFrame();
                }
            }
            catch { faulted = true; throw; }
            return packets.ToArray();
        }
        public void Complete()
        {
            if (faulted || headerCount != 0 || body != null) throw new InvalidDataException("Connection ended inside a packet.");
        }
        public void Reset() { ClearFrame(); faulted = false; }
        private void ClearFrame() { headerCount = bodyCount = 0; headerLength = 2; body = null; }
    }
}
