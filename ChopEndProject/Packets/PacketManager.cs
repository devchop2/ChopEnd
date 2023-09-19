using System;
using ServerCore;

namespace chopsFirstServer
{
    public class PacketManager
    {
        static PacketManager _instance = new PacketManager();
        public static PacketManager Instance { get { return _instance; } }

        public void OnRecvPacket(Session session, ArraySegment<byte> buffer, Action<Session, IPacket> handler)
        {

            if (buffer.Array == null) return;

            string recvData = BitConverter.ToString(buffer.Array, buffer.Offset, buffer.Count);
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += sizeof(ushort);
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(ushort);
        }
    }


    public interface IPacketHandler
    {

    }
}

