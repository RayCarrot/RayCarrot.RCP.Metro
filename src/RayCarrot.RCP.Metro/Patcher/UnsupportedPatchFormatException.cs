using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro.Patcher;

[Serializable]
public class UnsupportedPatchFormatException : Exception
{
    public UnsupportedPatchFormatException() { }
    public UnsupportedPatchFormatException(string? message) : base(message) { }
    public UnsupportedPatchFormatException(string? message, Exception inner) : base(message, inner) { }
    protected UnsupportedPatchFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}