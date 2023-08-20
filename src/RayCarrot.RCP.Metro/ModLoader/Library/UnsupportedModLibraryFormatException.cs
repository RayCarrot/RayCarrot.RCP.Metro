using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

[Serializable]
public class UnsupportedModLibraryFormatException : Exception
{
    public UnsupportedModLibraryFormatException() { }
    public UnsupportedModLibraryFormatException(string? message) : base(message) { }
    public UnsupportedModLibraryFormatException(string? message, Exception inner) : base(message, inner) { }
    protected UnsupportedModLibraryFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}