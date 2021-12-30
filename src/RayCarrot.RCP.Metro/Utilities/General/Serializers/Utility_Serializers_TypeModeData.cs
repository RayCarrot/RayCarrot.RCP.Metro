using System;
using BinarySerializer;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public record Utility_Serializers_TypeModeData
{
    public Utility_Serializers_TypeModeData()
    {
        InitContext = delegate { };
        Endian = Endian.Little;
        GetDefaultDir = () => Game?.GetInstallDir(false);
    }

    public Action<Context> InitContext { get; init; }
    public Games? Game { get; init; }
    public IStreamEncoder? Encoder { get; init; }
    public Endian Endian { get; init; }
    public Func<FileSystemPath?> GetDefaultDir { get; init; }
}