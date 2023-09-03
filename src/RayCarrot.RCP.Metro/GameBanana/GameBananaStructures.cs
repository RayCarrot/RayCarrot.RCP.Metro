using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro.GameBanana;

// These are classes for the different GameBanana API structures so that
// we can easily deserialize them. They do not include every possible
// property since not all are used here.

public record GameBananaSubfeed(
    [property: JsonProperty("_aMetadata", Required = Required.Always)] GameBananaSubfeedMetadata Metadata,
    [property: JsonProperty("_aRecords", Required = Required.Always)] GameBananaRecord[] Records);

public record GameBananaSubfeedMetadata(
    [property: JsonProperty("_nRecordCount", Required = Required.Always)] int RecordCount,
    [property: JsonProperty("_nPerpage", Required = Required.Always)] int PerPage,
    [property: JsonProperty("_bIsComplete", Required = Required.Always)] bool IsComplete);

public record GameBananaRecord(
    [property: JsonProperty("_idRow", Required = Required.Always)] int Id,
    [property: JsonProperty("_sName", Required = Required.Always)] string Name,
    [property: JsonProperty("_sProfileUrl")] string? Url,
    [property: JsonProperty("_tsDateAdded"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime DateAdded,
    [property: JsonProperty("_tsDateModified"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime DateModified,
    [property: JsonProperty("_bHasFiles")] bool HasFiles,
    [property: JsonProperty("_aPreviewMedia")] GameBananaMedia? PreviewMedia,
    [property: JsonProperty("_aSubmitter")] GameBananaMember? Submitter,
    [property: JsonProperty("_sVersion")] string? Version,
    [property: JsonProperty("_bIsObsolete")] bool IsObsolete,
    [property: JsonProperty("_nPostCount")] int PostCount,
    [property: JsonProperty("_nLikeCount")] int LikeCount,
    [property: JsonProperty("_nViewCount")] int ViewCount);

public record GameBananaMedia(
    [property: JsonProperty("_aImages")] GameBananaImage[]? Images);

public record GameBananaImage(
    [property: JsonProperty("_sType", Required = Required.Always)] string Type,
    [property: JsonProperty("_sBaseUrl", Required = Required.Always)] string BaseUrl,
    [property: JsonProperty("_sFile", Required = Required.Always)] string File,
    [property: JsonProperty("_sFile100")] string? File100,
    [property: JsonProperty("_hFile100")] int File100Height,
    [property: JsonProperty("_wFile100")] int File100Width,
    [property: JsonProperty("_sFile220")] string? File220,
    [property: JsonProperty("_hFile220")] int File220Height,
    [property: JsonProperty("_wFile220")] int File220Width,
    [property: JsonProperty("_sFile530")] string? File530,
    [property: JsonProperty("_hFile530")] int File530Height,
    [property: JsonProperty("_wFile530")] int File530Width);

public record GameBananaMember(
    [property: JsonProperty("_idRow", Required = Required.Always)] int Id,
    [property: JsonProperty("_sName", Required = Required.Always)] string Name,
    [property: JsonProperty("_bIsOnline")] bool IsOnline,
    [property: JsonProperty("_bHasRipe")] bool HasRipe,
    [property: JsonProperty("_sProfileUrl")] string? ProfileUrl,
    [property: JsonProperty("_sAvatarUrl")] string? AvatarUrl);

public record GameBananaMod(
    [property: JsonProperty("_idRow")] int Id,
    [property: JsonProperty("_bIsPrivate")] bool IsPrivate,
    [property: JsonProperty("_tsDateModified"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime DateModified,
    [property: JsonProperty("_tsDateAdded"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime DateAdded,
    [property: JsonProperty("_aPreviewMedia")] GameBananaMedia? PreviewMedia,
    [property: JsonProperty("_bIsTrashed")] bool IsTrashed,
    [property: JsonProperty("_bIsWithheld")] bool IsWithheld,
    [property: JsonProperty("_sName")] string? Name,
    [property: JsonProperty("_bCreatedBySubmitter")] bool CreatedBySubmitter,
    [property: JsonProperty("_nDownloadCount")] int DownloadCount,
    [property: JsonProperty("_aFiles")] GameBananaFile[]? Files,
    // Can't set the type here since if it's empty then it's an array [] and if not an object {}
    [property: JsonProperty("_aModManagerIntegrations")] object? ModManagerIntegrations,
    [property: JsonProperty("_sDescription")] string? Description,
    [property: JsonProperty("_sText")] string? Text,
    [property: JsonProperty("_bIsObsolete")] bool IsObsolete,
    [property: JsonProperty("_nLikeCount")] int LikeCount,
    [property: JsonProperty("_nViewCount")] int ViewCount,
    [property: JsonProperty("_sVersion")] string? Version,
    [property: JsonProperty("_aSubmitter")] GameBananaMember? Submitter);

public record GameBananaFile(
    [property: JsonProperty("_idRow", Required = Required.Always)] int Id,
    [property: JsonProperty("_aModManagerIntegrations")] GameBananaModManager[]? ModManagerIntegrations,
    [property: JsonProperty("_nDownloadCount")] int DownloadCount,
    [property: JsonProperty("_nFilesize")] int FileSize,
    [property: JsonProperty("_sDescription")] string? Description,
    [property: JsonProperty("_sDownloadUrl", Required = Required.Always)] string DownloadUrl,
    [property: JsonProperty("_sFile", Required = Required.Always)] string File,
    [property: JsonProperty("_sMd5Checksum")] string Md5Checksum,
    [property: JsonProperty("_tsDateAdded"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime DateAdded);

public record GameBananaModManager(
    [property: JsonProperty("_aGameRowIds")] int[]? GameIds,
    [property: JsonProperty("_idToolRow", Required = Required.Always)] int ToolId,
    [property: JsonProperty("_sDownloadUrl")] string? DownloadUrl,
    [property: JsonProperty("_sIconUrl")] string? IconUrl,
    [property: JsonProperty("_sInstallerName")] string? InstallerName,
    [property: JsonProperty("_sInstallerUrl")] string? InstallerUrl);