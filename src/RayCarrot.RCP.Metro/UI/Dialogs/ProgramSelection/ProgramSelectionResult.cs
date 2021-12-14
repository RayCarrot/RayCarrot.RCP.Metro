using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Result for a program selection dialog
/// </summary>
public class ProgramSelectionResult : UserInputResult
{
    /// <summary>
    /// The program file path
    /// </summary>
    public FileSystemPath ProgramFilePath { get; set; }
}