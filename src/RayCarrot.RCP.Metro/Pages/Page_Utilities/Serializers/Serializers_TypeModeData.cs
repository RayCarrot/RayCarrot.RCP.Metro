using System;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public record Serializers_TypeModeData
{
    public Action<Context> InitContext { get; init; } = delegate { };
    public Games? Game { get; init; }
    public IStreamEncoder? Encoder { get; init; }
    public Endian Endian { get; init; } = Endian.Little;
}