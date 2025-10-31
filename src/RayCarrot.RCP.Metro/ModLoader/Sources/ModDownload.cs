namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public record ModDownload(string FileName, string ModName, string DownloadUrl, long FileSize, object? InstallData);