using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro.ModLoader;

[Serializable]
public class UnsupportedModVersionException : Exception
{
    public UnsupportedModVersionException() { }
    public UnsupportedModVersionException(string? message) : base(message) { }
    public UnsupportedModVersionException(string? message, Exception inner) : base(message, inner) { }
    protected UnsupportedModVersionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}