namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public record OpenModDownloadPageMessage(GameInstallation GameInstallation, string? SourceId, object? InstallData);