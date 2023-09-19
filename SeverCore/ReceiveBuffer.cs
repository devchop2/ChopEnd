using System;
namespace ServerCore
{
    public class ReceiveBuffer
    {
        ArraySegment<byte> _buffer;

        int _readIndex;
        int _writeIndex;

        public ReceiveBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writeIndex - _readIndex; } }
        public int FreeSize { get { return _buffer.Count - _writeIndex; } }

        public ArraySegment<byte> ReadSegment
        {
            get
            {
                if (_buffer.Array == null) return new ArraySegment<byte>();
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readIndex, DataSize);
            }
        }
        public ArraySegment<byte> WriteSegment
        {
            get
            {
                if (_buffer.Array == null) return new ArraySegment<byte>();
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writeIndex, FreeSize);
            }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (_buffer.Array == null) return;

            if (DataSize == 0)
            {
                _readIndex = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readIndex, _buffer.Array, _buffer.Offset, DataSize);
            }
        }

        //데이터 조립이 완료되어서 읽기처리를 끝냈을때 호출
        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize) return false;
            _readIndex += numOfBytes;
            return true;
        }

        //통신이 와서 버퍼에 write처리를 완료하였을때 호출
        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize) return false;
            _writeIndex += numOfBytes;
            return true;
        }



        public ArraySegment<byte> GetPacketData()
        {
            int HeaderSize = 2;

            if (_buffer.Array == null || DataSize < HeaderSize) return new ArraySegment<byte>();
            if (DataSize < HeaderSize) return new ArraySegment<byte>();

            int packetSize = BitConverter.ToInt16(_buffer.Array, _buffer.Offset + _readIndex);
            if (DataSize < packetSize) return new ArraySegment<byte>(); //not received yet

            var validData = new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readIndex, packetSize);

            OnRead(packetSize);
            return validData;
        }
    }
}

