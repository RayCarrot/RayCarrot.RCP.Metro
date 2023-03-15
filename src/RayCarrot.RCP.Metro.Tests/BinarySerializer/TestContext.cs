using BinarySerializer;

namespace RayCarrot.RCP.Metro.Tests;

public class TestContext : Context
{
    public TestContext() : base(String.Empty, new TestSerializerSettings(), null, new TestFileManager()) { }
}