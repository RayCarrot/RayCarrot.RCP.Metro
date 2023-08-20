using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro.ModLoader;

[Serializable]
public class InvalidModException : Exception
{
    public InvalidModException() { }
    public InvalidModException(string? message) : base(message) { }
    public InvalidModException(string? message, Exception inner) : base(message, inner) { }
    protected InvalidModException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}