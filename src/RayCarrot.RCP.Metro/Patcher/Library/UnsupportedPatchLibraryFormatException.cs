using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro.Patcher.Library;

[Serializable]
public class UnsupportedPatchLibraryFormatException : Exception
{
    public UnsupportedPatchLibraryFormatException() { }
    public UnsupportedPatchLibraryFormatException(string? message) : base(message) { }
    public UnsupportedPatchLibraryFormatException(string? message, Exception inner) : base(message, inner) { }
    protected UnsupportedPatchLibraryFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}