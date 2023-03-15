using BinarySerializer;

namespace RayCarrot.RCP.Metro.Tests;

public class BinaryDeserializerTests
{
    private static void TestOnBuffer(byte[] data, Action<BinaryDeserializer> testAction)
    {
        using TestContext context = new();
        BinaryFile file = context.AddFile(new StreamFile(context, "TestFile", new MemoryStream(data)));
        BinaryDeserializer s = context.Deserializer;
        s.Goto(file.StartPointer);
        testAction(s);
    }

    [Fact]
    public void Serialize_Boolean()
    {
        byte[] buffer = { 0x00, 0x01, 0x02, 0xFF, };

        TestOnBuffer(buffer, s =>
        {
            Assert.False(s.Serialize<bool>(default));
            Assert.True(s.Serialize<bool>(default));
            Assert.True(s.Serialize<bool>(default));
            Assert.True(s.Serialize<bool>(default));
        });
    }

    [Fact]
    public void Serialize_SByte()
    {
        byte[] buffer = { 0x00, 0x01, 0xFF, };

        TestOnBuffer(buffer, s =>
        {
            Assert.Equal(0, s.Serialize<sbyte>(default));
            Assert.Equal(1, s.Serialize<sbyte>(default));
            Assert.Equal(-1, s.Serialize<sbyte>(default));
        });
    }

    [Fact]
    public void Serialize_Byte()
    {
        byte[] buffer = { 0x00, 0x01, 0xFF, };

        TestOnBuffer(buffer, s =>
        {
            Assert.Equal(0, s.Serialize<byte>(default));
            Assert.Equal(1, s.Serialize<byte>(default));
            Assert.Equal(255, s.Serialize<byte>(default));
        });
    }

    [Fact]
    public void Serialize_EndOfStream()
    {
        byte[] buffer =
        {
            0x01
        };

        TestOnBuffer(buffer, s =>
        {
            Assert.Throws(typeof(EndOfStreamException), () => s.Serialize<int>(default));
        });
    }

    // TODO: Add more tests
}