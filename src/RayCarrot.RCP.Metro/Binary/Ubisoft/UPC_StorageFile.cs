#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class UPC_StorageFile<T> : BinarySerializable
    where T : BinarySerializable, new()
{
    public uint HeaderSize { get; set; } // 0x24, 0x224 or 0x404. The size determines how the header is parsed.
    public byte[] Header { get; set; }
    public T Content { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        HeaderSize = s.Serialize<uint>(HeaderSize, name: nameof(HeaderSize));
        Header = s.SerializeArray<byte>(Header, HeaderSize, name: nameof(Header));
        Content = s.SerializeObject<T>(Content, name: nameof(Content));
    }
}