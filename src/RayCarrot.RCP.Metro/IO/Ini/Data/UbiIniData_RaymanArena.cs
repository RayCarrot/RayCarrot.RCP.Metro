#nullable disable
namespace RayCarrot.RCP.Metro.Ini;

/// <summary>
/// Handles the Rayman Arena section of a ubi.ini file
/// </summary>
public class UbiIniData_RaymanArena : UbiIniData_RaymanM
{
    #region Constructor

    /// <summary>
    /// Constructor for a custom section name
    /// </summary>
    /// <param name="path">The path of the ubi.ini file</param>
    /// <param name="sectionKey">The name of the section to retrieve</param>
    public UbiIniData_RaymanArena(string path, string sectionKey) : base(path, sectionKey)
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="path">The path of the ubi.ini file</param>
    public UbiIniData_RaymanArena(string path) : base(path, SectionName)
    {

    }

    #endregion

    #region Constant Fields

    /// <summary>
    /// The section name
    /// </summary>
    public new const string SectionName = "Rayman Arena";

    #endregion

    #region Properties

    /// <summary>
    /// The ModemQuality key
    /// </summary>
    public string ModemQuality
    {
        get => Section?["ModemQuality"];
        set => Section["ModemQuality"] = value;
    }

    /// <summary>
    /// The UDPPort key
    /// </summary>
    public string UDPPort
    {
        get => Section?["UDPPort"];
        set => Section["UDPPort"] = value;
    }

    #endregion

    #region Formatted Properties

    /// <summary>
    /// The formatted Language key
    /// </summary>
    public RALanguages? FormattedRALanguage => Enum.TryParse(Language, out RALanguages r2Languages) ? (RALanguages?)r2Languages : null;

    /// <summary>
    /// The formatted ModemQuality key
    /// </summary>
    public int? FormattedModemQuality => Int32.TryParse(ModemQuality, out int result) ? (int?)result : null;

    /// <summary>
    /// The formatted UDPPort key
    /// </summary>
    public int? FormattedUDPPort => Int32.TryParse(UDPPort, out int result) ? (int?)result : null;

    #endregion
}