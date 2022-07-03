namespace RayCarrot.RCP.Metro.Archive;

public record PatchContainerManifest(PatchHistoryManifest History, PatchManifest[] Patches, string[]? EnabledPatches, int ContainerVersion);