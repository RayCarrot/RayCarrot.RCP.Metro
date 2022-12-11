#nullable disable
namespace RayCarrot.RCP.Metro.Ini;

/// <summary>
/// Handles the Rayman 2 section of a ubi.ini file
/// </summary>
public class UbiIniData_Rayman2 : UbiIniData
{
    #region Constructor

    /// <summary>
    /// Constructor for a custom section name
    /// </summary>
    /// <param name="path">The path of the ubi.ini file</param>
    /// <param name="sectionKey">The name of the section to retrieve</param>
    public UbiIniData_Rayman2(string path, string sectionKey) : base(path, sectionKey)
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="path">The path of the ubi.ini file</param>
    public UbiIniData_Rayman2(string path) : base(path, SectionName)
    {
    }

    #endregion

    #region Constant Fields

    /// <summary>
    /// The section name
    /// </summary>
    public const string SectionName = "Rayman2";

    #endregion

    #region Properties

    /// <summary>
    /// The GLI_DllFile key
    /// </summary>
    public string GLI_DllFile
    {
        get => Section?["GLI_DllFile"];
        set => Section["GLI_DllFile"] = value;
    }

    /// <summary>
    /// The GLI_Dll key
    /// </summary>
    public string GLI_Dll
    {
        get => Section?["GLI_Dll"];
        set => Section["GLI_Dll"] = value;
    }

    /// <summary>
    /// The GLI_Driver key
    /// </summary>
    public string GLI_Driver
    {
        get => Section?["GLI_Driver"];
        set => Section["GLI_Driver"] = value;
    }

    /// <summary>
    /// The GLI_Device key
    /// </summary>
    public string GLI_Device
    {
        get => Section?["GLI_Device"];
        set => Section["GLI_Device"] = value;
    }

    /// <summary>
    /// The GLI_Mode key
    /// </summary>
    public string GLI_Mode
    {
        get => Section?["GLI_Mode"];
        set => Section["GLI_Mode"] = value;
    }

    /// <summary>
    /// The Language key
    /// </summary>
    public string Language
    {
        get => Section?["Language"];
        set => Section["Language"] = value;
    }

    /// <summary>
    /// The ParticuleRate key
    /// </summary>
    public string ParticuleRate
    {
        get => Section?["ParticuleRate"];
        set => Section["ParticuleRate"] = value;
    }

    #endregion

    #region Formatted Properties

    /// <summary>
    /// The formatted GLI_Mode key
    /// </summary>
    public RayGLI_Mode FormattedGLI_Mode => RayGLI_Mode.Parse(GLI_Mode);

    /// <summary>
    /// The formatted Language key
    /// </summary>
    public R2Languages? FormattedLanguage => Enum.TryParse(Language, out R2Languages r2Languages) ? (R2Languages?)r2Languages : null;

    /// <summary>
    /// The formatted ParticuleRate key
    /// </summary>
    public R2ParticuleRate? FormattedParticuleRate => Enum.TryParse(ParticuleRate, out R2ParticuleRate r2ParticuleRate) ? (R2ParticuleRate?)r2ParticuleRate : null;

    #endregion
}