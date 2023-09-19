using System.Text;

public enum PacketID
{
    C_Login = 1,
}

public interface IPacket
{
    ushort Protocol { get; }
    void Deserialize(ArraySegment<byte> data);
    ArraySegment<byte> Serialize();
}

public class C_Login : IPacket
{
    public string nickname = "";

    public ushort Protocol => (ushort)PacketID.C_Login;

    public ArraySegment<byte> Serialize()
    {

        byte[] buffer = new byte[4096];
        ArraySegment<byte> segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        if (segment.Array == null) return new ArraySegment<byte>();

        ushort count = 0;
        bool success = true;

        count += sizeof(ushort); //size

        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Login); ;
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(s, count);

        ushort nicknameLen = (ushort)Encoding.Unicode.GetBytes(nickname, 0, nickname.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nicknameLen);

        count += sizeof(ushort);
        count += nicknameLen;

        if (!success) return new ArraySegment<byte>();
        return new ArraySegment<byte>(buffer, 0, count);

    }

    public void Deserialize(ArraySegment<byte> data)
    {

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(data.Array, data.Offset, data.Count);
        ushort count = 0;

        count += sizeof(ushort); //size
        count += sizeof(ushort); //packetId

        ushort nicknameLen = (ushort)BitConverter.ToInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        nickname = Encoding.Unicode.GetString(s.Slice(count, nicknameLen));
        count += nicknameLen;
    }
}
