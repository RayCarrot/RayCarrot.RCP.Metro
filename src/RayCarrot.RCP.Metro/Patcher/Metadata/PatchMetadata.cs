using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher.Metadata;

/// <summary>
/// Defines metadata for a patch
/// </summary>
/// <param name="Id">The unique patch id</param>
/// <param name="Games">The ids of the games this patch targets</param>
/// <param name="Format">The patch metadata format version</param>
/// <param name="Name">The patch display name</param>
/// <param name="Description">The patch description</param>
/// <param name="Author">The patch author(s)</param>
/// <param name="Website">A website URL for the patch</param>
/// <param name="Version">The current patch version</param>
/// <param name="Changelog">The patch changelog entries</param>
public record PatchMetadata(
    [property: JsonProperty("id", Required = Required.Always)] string Id,
    [property: JsonProperty("games", Required = Required.Always)] string[] Games,
    [property: JsonProperty("format", Required = Required.Always)] int Format,

    [property: JsonProperty("name")] string? Name,
    [property: JsonProperty("description")] string? Description,
    [property: JsonProperty("author")] string? Author,
    [property: JsonProperty("website")] string? Website,

    [property: JsonProperty("version")] PatchVersion? Version,
    [property: JsonProperty("changelog")] PatchChangelogEntry[]? Changelog)
{
    public bool IsGameValid(GameDescriptor gameDescriptor)
    {
        return Games.Any(x => x == gameDescriptor.GameId);
    }
}