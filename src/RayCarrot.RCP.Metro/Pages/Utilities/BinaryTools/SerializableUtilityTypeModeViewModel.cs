﻿using BinarySerializer;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

public class SerializableUtilityTypeModeViewModel : BaseViewModel
{
    public SerializableUtilityTypeModeViewModel(string displayName) : this(null, displayName) { }

    public SerializableUtilityTypeModeViewModel(Enum? gameMode, string? displayName = null)
    {
        GameModeBaseAttribute? attr = gameMode?.GetAttribute<GameModeBaseAttribute>();

        GameMode = gameMode;
        DisplayName = displayName ?? attr?.DisplayName ?? "NULL";

        GetDefaultDir = g =>
        {
            if (gameMode == null)
                return FileSystemPath.EmptyPath;

            return GameModeHelpers.FindGameInstallation(g, gameMode)?.InstallLocation.Directory ?? FileSystemPath.EmptyPath;
        };
    }

    protected Enum? GameMode { get; }

    public string DisplayName { get; }
    public IStreamEncoder? Encoder { get; init; }
    public Func<GamesManager, FileSystemPath> GetDefaultDir { get; init; }

    public object? GetSettings() => GameMode?.GetAttribute<GameModeBaseAttribute>()?.GetSettingsObject();

    public void InitContext(Context context)
    {
        object? settings = GetSettings();

        if (settings is not null)
            context.AddSettings(settings, settings.GetType());
    }
}