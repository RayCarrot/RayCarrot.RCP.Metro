#nullable disable
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Result for editing an educational DOS game
/// </summary>
public class EducationalDosGameEditResult : UserInputResult
{
    #region Public Properties

    /// <summary>
    /// The selected mount path
    /// </summary>
    public FileSystemPath MountPath { get; set; }

    /// <summary>
    /// The selected launch mode
    /// </summary>
    public string LaunchMode { get; set; }

    /// <summary>
    /// The selected name
    /// </summary>
    public string Name { get; set; }

    #endregion
}