using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro.Patcher;

[Serializable]
public class InvalidPatchException : Exception
{
    public InvalidPatchException() { }
    public InvalidPatchException(string? message) : base(message) { }
    public InvalidPatchException(string? message, Exception inner) : base(message, inner) { }
    protected InvalidPatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}