using System;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record ExternalPatchManifest(
    [property: JsonProperty(Required = Required.Always)]
    string ID,

    [property: JsonProperty(Required = Required.Always)]
    int FileVersion,

    string? Name,
    string? Description,
    string? Author,

    long TotalSize,
    DateTime ModifiedDate,

    int Revision,

    int AddedFilesCount,
    int RemovedFilesCount,

    string Patch,
    long PatchSize,
    string? Thumbnail);