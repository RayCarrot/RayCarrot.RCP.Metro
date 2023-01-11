using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro.Games.Data;

/// <summary>
/// The available modes to launch Rayman Raving Rabbids 2 in
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RaymanRavingRabbids2LaunchMode
{
    /// <summary>
    /// All modes are included
    /// </summary>
    AllGames,

    /// <summary>
    /// The orange set is available
    /// </summary>
    Orange,

    /// <summary>
    /// The red set is available
    /// </summary>
    Red,

    /// <summary>
    /// The green set is available
    /// </summary>
    Green,

    /// <summary>
    /// The blue set is available
    /// </summary>
    Blue
}