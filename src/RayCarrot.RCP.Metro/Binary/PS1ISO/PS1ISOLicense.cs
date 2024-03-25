#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class PS1ISOLicense : BinarySerializable
{
    public string Text { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        // NOTE: The license text is actually longer, but the length is different per region and not always
        //       null-terminated. So we just read the common part of the text here for a general check.
        Text = s.SerializeString(Text, 59, name: nameof(Text));

        if (Text != "          Licensed  by          Sony Computer Entertainment")
            throw new BinarySerializableException(this, "ISO license isn't valid");
    }
}