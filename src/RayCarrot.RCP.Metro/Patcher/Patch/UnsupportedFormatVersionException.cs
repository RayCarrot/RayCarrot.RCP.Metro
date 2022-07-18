using System;
using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro.Patcher;

// TODO-UPDATE: Move to BinarySerializer library 

[Serializable]
public class UnsupportedFormatVersionException : Exception
{
    public UnsupportedFormatVersionException() { }
    public UnsupportedFormatVersionException(string message) : base(message) { }
    public UnsupportedFormatVersionException(string message, Exception inner) : base(message, inner) { }
    protected UnsupportedFormatVersionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}