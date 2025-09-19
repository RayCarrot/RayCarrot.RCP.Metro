using System.Text;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Unity_PlayerPrefsEntry : BinarySerializable
{
    public byte Type { get; set; }
    public int KeyLength { get; set; }
    public string Key { get; set; }

    public int IntValue { get; set; }
    public float FloatValue { get; set; }
    public int StringValueLength { get; set; }
    public string StringValue { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Type = s.Serialize<byte>(Type, name: nameof(Type));
        KeyLength = s.Serialize<int>(KeyLength, name: nameof(KeyLength));
        Key = s.SerializeString(Key, KeyLength, Encoding.UTF8, name: nameof(Key));

        switch ((char)Type)
        {
            case 'n':
                IntValue = s.Serialize<int>(IntValue, name: nameof(IntValue));
                break;

            case 'f':
                FloatValue = s.Serialize<float>(FloatValue, name: nameof(FloatValue));
                break;

            case 's':
                StringValueLength = s.Serialize<int>(StringValueLength, name: nameof(StringValueLength));
                StringValue = s.SerializeString(StringValue, StringValueLength, Encoding.UTF8, name: nameof(StringValue));
                break;

            default:
                throw new BinarySerializableException(this, $"Unsupported data type {Type}");
        }
    }
}