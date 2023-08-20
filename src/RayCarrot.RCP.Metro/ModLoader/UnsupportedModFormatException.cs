using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro.ModLoader;

[Serializable]
public class UnsupportedModFormatException : Exception
{
    public UnsupportedModFormatException() { }
    public UnsupportedModFormatException(string? message) : base(message) { }
    public UnsupportedModFormatException(string? message, Exception inner) : base(message, inner) { }
    protected UnsupportedModFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}