using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record ExternalPatchMetaData(
    [property: JsonProperty("id")] string? Id,
    [property: JsonProperty("formatVersion")] int? FormatVersion,

    [property: JsonProperty("gameIds")] string[]? GameIds,

    [property: JsonProperty("name")] string? Name,
    [property: JsonProperty("description")] string? Description,
    [property: JsonProperty("author")] string? Author,
    [property: JsonProperty("website")] string? Website,

    [property: JsonProperty("totalSize")] long? TotalSize, // Uncompressed data
    [property: JsonProperty("fileSize")] long? FileSize, // Compressed .gp file
    [property: JsonProperty("modifiedDate")] DateTime? ModifiedDate,

    [property: JsonProperty("version")] PatchVersion? Version,
    [property: JsonProperty("changelogEntries")] ExternalPatchChangeLogEntry?[]? ChangelogEntries,

    [property: JsonProperty("addedFilesCount")] int? AddedFilesCount,
    [property: JsonProperty("removedFilesCount")] int? RemovedFilesCount,

    [property: JsonProperty("patchUrl")] string? PatchUrl,
    [property: JsonProperty("thumbUrl")] string? ThumbnailUrl);