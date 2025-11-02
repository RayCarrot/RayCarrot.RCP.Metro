using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro.ModLoader.Metadata;

/// <summary>
/// Defines metadata for a mod
/// </summary>
/// <param name="Id">The unique mod id</param>
/// <param name="Games">The ids of the games this mod targets</param>
/// <param name="Format">The mod metadata format version</param>
/// <param name="Name">The mod display name</param>
/// <param name="Description">The mod description</param>
/// <param name="Author">The mod author(s)</param>
/// <param name="Website">A website URL for the mod</param>
/// <param name="Version">The current mod version</param>
/// <param name="Changelog">The mod changelog entries</param>
/// <param name="Archives">The game archive files which this mod has data defined for</param>
public record ModMetadata(
    [property: JsonProperty("id", Required = Required.Always)] string Id,
    [property: JsonProperty("games", Required = Required.Always)] string[] Games,
    [property: JsonProperty("format", Required = Required.Always)] int Format,
    [property: JsonProperty("min_app_version", ItemConverterType = typeof(VersionConverter))] Version? MinAppVersion, // Format 1

    [property: JsonProperty("name")] string? Name,
    [property: JsonProperty("description")] string? Description,
    [property: JsonProperty("author")] string? Author,
    [property: JsonProperty("website")] string? Website,

    [property: JsonProperty("version")] ModVersion? Version,
    [property: JsonProperty("changelog")] ModChangelogEntry[]? Changelog,

    [property: JsonProperty("archives")] ModArchiveInfo[]? Archives,

    [property: JsonProperty("dependencies")] ModDependencyInfo[]? Dependencies) // Format 1
{
    public bool IsGameValid(GameDescriptor gameDescriptor)
    {
        return Games.Any(x => x == gameDescriptor.GameId);
    }
}