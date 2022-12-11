using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record ExternalPatchManifest(
    [property: JsonProperty(Required = Required.Always)]
    string ID,

    [property: JsonProperty(Required = Required.Always)]
    int FormatVersion,

    Version MinAppVersion, // Added in version 13.4.0

    string? Name,
    string? Description,
    string? Author,
    string? Website,

    long TotalSize,
    DateTime ModifiedDate,

    PatchVersion Version,

    int AddedFilesCount,
    int RemovedFilesCount,

    string Patch,
    long PatchSize,
    string? Thumbnail);