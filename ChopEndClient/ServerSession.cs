namespace ChopEnd
{
    using System;
    using System.Net;
    using ServerCore;

    public class ServerSession : Session
    {
        public bool IsConnected { get; private set; } = false;
        public override void OnConnected(EndPoint? point)
        {
            IsConnected = true;
        }

        public override void OnDisConnected(EndPoint? point)
        {
            IsConnected = false;
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            Console.WriteLine("Recv packet. " + buffer.ToString());
        }

        public override void OnSend(int numOfBytes)
        {

        }
    }
}

