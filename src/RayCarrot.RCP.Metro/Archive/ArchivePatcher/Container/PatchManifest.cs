namespace RayCarrot.RCP.Metro.Archive;

public record PatchManifest(PatchHistoryManifest History, PatchManifestItem[] Patches, int ContainerVersion);