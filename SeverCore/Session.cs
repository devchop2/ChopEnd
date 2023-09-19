using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public abstract class Session
    {
        Socket? _socket;
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        object pendingLock = new object();
        int _disconnected = 0;

        ReceiveBuffer _recvBuffer = new ReceiveBuffer(short.MaxValue);

        #region Handler
        public abstract void OnConnected(EndPoint? point);
        public abstract void OnDisConnected(EndPoint? point);
        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        #endregion

        public void Start(Socket socket)
        {
            _socket = socket;
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();

        }

        #region Send

        public void Send(ArraySegment<byte> contents)
        {
            if (contents.Count == 0) return;

            lock (pendingLock)
            {
                _sendQueue.Enqueue(contents);
                if (pendingList.Count == 0) RegisterSend();
            }
        }

        public void Send(List<ArraySegment<byte>> contents)
        {
            if (contents == null || contents.Count == 0) return;
            lock (pendingLock)
            {
                foreach (var item in contents)
                {
                    _sendQueue.Enqueue(item);
                }
                if (pendingList.Count == 0) RegisterSend();
            }
        }

        void RegisterSend()
        {
            if (_socket == null || _disconnected == 1) return;
            //쌓여잇던 queue를 비우고 sendArgs에 넣어서 보내기.
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                pendingList.Add(buff);
            }

            try
            {
                sendArgs.BufferList = pendingList;
                bool pending = _socket.SendAsync(sendArgs);
                if (!pending) OnSendCompleted(null, sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Register send exceiption occurred.  " + e.Message);
            }

        }

        void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
        {
            lock (pendingLock)
            {
                int sendBytes = args.BytesTransferred;
                if (sendBytes > 0 && args.SocketError == SocketError.Success)
                {
                    sendArgs.BufferList = null;
                    pendingList.Clear();

                    OnSend(sendBytes);
                    if (_sendQueue.Count > 0) RegisterSend();

                }
            }
        }
        #endregion

        #region Recv
        void RegisterRecv()
        {
            if (_socket == null || _disconnected == 1) return;

            _recvBuffer.Clean();
            ArraySegment<byte> writeSegment = _recvBuffer.WriteSegment;
            recvArgs.SetBuffer(writeSegment.Array, writeSegment.Offset, writeSegment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(recvArgs);
                if (!pending) OnRecvCompleted(null, recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Register Recv exception occurred." + e.Message);
            }
        }

        void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            int recvBytes = args.BytesTransferred;
            if (recvBytes <= 0 || args.SocketError != SocketError.Success) return;

            try
            {
                bool writeSuccess = _recvBuffer.OnWrite(recvBytes);
                if (!writeSuccess)
                {
                    //무언가 문제가 발생한것임 
                    Disconnected();
                    return;
                }

                while (true)
                {
                    var packet = _recvBuffer.GetPacketData();
                    if (packet.Count == 0) break;

                    OnRecvPacket(packet);
                }

                RegisterRecv();
            }
            catch (Exception e)
            {
                Console.WriteLine("on receive fail. " + e.Message);
                Disconnected();
            }
        }


        int OnRecv(ArraySegment<byte> buffer)
        {
            int HeaderSize = 2;
            int processLen = 0;
            while (true)
            {
                if (buffer.Array == null || buffer.Count < HeaderSize) break;

                int dataSize = BitConverter.ToInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) break; //not recved all.

                var validData = new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize);
                processLen += dataSize;

                OnRecvPacket(validData);
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }
        #endregion

        #region Connection
        public void Disconnected()
        {
            //중복호출 방지
            if (_socket == null) return;
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

            OnDisConnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();

        }
        void Clear()
        {
            lock (pendingList)
            {
                _sendQueue.Clear();
                pendingList.Clear();
            }
        }
        #endregion
    }
}

