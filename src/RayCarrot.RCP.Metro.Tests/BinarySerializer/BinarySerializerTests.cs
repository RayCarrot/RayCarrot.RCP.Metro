using BinarySerializer;

namespace RayCarrot.RCP.Metro.Tests;

public class BinarySerializerTests
{
    private static byte[] TestOnBuffer(Action<BinarySerializer.BinarySerializer> testAction)
    {
        using TestContext context = new();
        MemoryStream stream = new();
        BinaryFile file = context.AddFile(new StreamFile(context, "TestFile", stream));
        BinarySerializer.BinarySerializer s = context.Serializer;
        s.Goto(file.StartPointer);
        testAction(s);
        return stream.ToArray();
    }

    [Fact]
    public void DoBits_SignedInt()
    {
        // Previously there was a bug when using a signed int and setting
        // the highest bit. This test verifies that has been fixed.

        byte[] buffer = TestOnBuffer(s =>
        {
            s.DoBits<int>(b =>
            {
                for (int i = 0; i < 32; i++)
                    b.SerializeBits<byte>(1, 1);
            });
        });

        if (buffer.Any(x => x != 0xFF))
            throw new Exception();
    }

    // TODO: Add more tests
}