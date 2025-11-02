namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public record RemovedModFromLibraryMessage(GameInstallation GameInstallation, string? SourceId, object? InstallData);