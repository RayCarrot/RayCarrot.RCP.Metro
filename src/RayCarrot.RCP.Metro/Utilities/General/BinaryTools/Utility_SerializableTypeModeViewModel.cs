using System;
using BinarySerializer;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class Utility_SerializableTypeModeViewModel : BaseViewModel
{
    public Utility_SerializableTypeModeViewModel(string displayName, Games? game = null) : this(null, displayName, game) { }

    public Utility_SerializableTypeModeViewModel(Enum? gameMode, string? displayName = null, Games? game = null)
    {
        GameModeBaseAttribute? attr = gameMode?.GetAttribute<GameModeBaseAttribute>();

        GameMode = gameMode;
        DisplayName = displayName ?? attr?.DisplayName ?? "NULL";
        Game = game ?? attr?.Game;
        GetDefaultDir = () => Game?.GetInstallDir(false);
    }

    protected Enum? GameMode { get; }

    public string DisplayName { get; }
    public Games? Game { get; }
    public IStreamEncoder? Encoder { get; init; }
    public Func<FileSystemPath?> GetDefaultDir { get; init; }

    public object? GetSettings() => GameMode?.GetAttribute<GameModeBaseAttribute>()?.GetSettingsObject();

    public void InitContext(Context context)
    {
        object? settings = GetSettings();

        if (settings is not null)
            context.AddSettings(settings, settings.GetType());
    }
}