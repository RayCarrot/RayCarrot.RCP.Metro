using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher.Library;

/// <summary>
/// General metadata for the patch library
/// </summary>
/// <param name="Format">The current format version. Used to ensure newer versions of the library won't be modified by older versions of the program.</param>
public record LibraryMetadata(
    [property: JsonProperty("format", Required = Required.Always)] int Format);