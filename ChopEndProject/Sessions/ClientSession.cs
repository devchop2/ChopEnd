using System;
using System.Net;
using ServerCore;

namespace chopsFirstServer
{
    public class ClientSession : Session
    {
        string nickname = "";
        GameRoom? room;
        public int sessionId { get; private set; }

        public ClientSession(int sessionId)
        {
            this.sessionId = sessionId;
        }

        public override void OnConnected(EndPoint? point)
        {
            Console.WriteLine("[Client Session] Connected.");
            room = null;
        }

        public override void OnDisConnected(EndPoint? point)
        {
            Console.WriteLine("[Client Session] Disconnected nickname:" + nickname);
        }


        public override void OnSend(int numOfBytes)
        {
            throw new NotImplementedException();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            if (buffer.Array == null) return;
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"RecvPacket. id:${id}, dataSize:{dataSize}");
        }
    }
}

