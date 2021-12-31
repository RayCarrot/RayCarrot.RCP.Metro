using System;
using BinarySerializer;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public record Utility_SerializableTypeModeData
{
    public Utility_SerializableTypeModeData()
    {
        Endian = Endian.Little;
        GetDefaultDir = () => Game?.GetInstallDir(false);
    }

    public Func<object>? GetSettings { get; init; }
    public Games? Game { get; init; }
    public IStreamEncoder? Encoder { get; init; }
    public Endian Endian { get; init; }
    public Func<FileSystemPath?> GetDefaultDir { get; init; }

    public void InitContext(Context context)
    {
        if (GetSettings is not null)
        {
            object settings = GetSettings();
            context.AddSettings(settings, settings.GetType());
        }
    }
}