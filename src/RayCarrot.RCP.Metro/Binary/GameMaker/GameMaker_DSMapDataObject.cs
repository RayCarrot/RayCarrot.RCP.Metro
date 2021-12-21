#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class GameMaker_DSMapDataObject : BinarySerializable
{
    public ObjectType Type { get; set; }

    public double NumberValue { get; set; }
    public int NumberValueAsInt => (int)NumberValue;
    public bool NumberValueAsBool => NumberValue > 0;

    public int StringLength { get; set; }
    public string StringValue { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Type = s.Serialize<ObjectType>(Type, name: nameof(Type));

        switch (Type)
        {
            case ObjectType.Number:
                NumberValue = s.Serialize<double>(NumberValue, name: nameof(NumberValue));
                break;

            case ObjectType.String:
                StringLength = s.Serialize<int>(StringLength, name: nameof(StringLength));
                StringValue = s.SerializeString(StringValue, StringLength, name: nameof(StringValue));
                break;

            default:
                throw new BinarySerializableException(this, $"Unsupported object type {Type}");
        }
    }

    public enum ObjectType
    {
        Number = 0,
        String = 1,
    }
}