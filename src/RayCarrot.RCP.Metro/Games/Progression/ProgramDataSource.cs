namespace RayCarrot.RCP.Metro;

/// <summary>
/// The data source to use for program data files. If the files are being written to a protected directory they might be redirected
/// to VirtualStore to get around the UAC requirement. This solution by the operation system can cause there to be two versions of the
/// same file, with the one being used depending on if the program is being run as admin or not.
/// Prior to version 12.2.0 the backup tool would for RRR use the same setting as <see cref="ProgramDataSource.Auto"/> while all other
/// games were hard-coded to use a non-VirtualStore location at all times.
/// </summary>
public enum ProgramDataSource
{
    /// <summary>
    /// Reading data will use the location with the most recently modified files while writing will write to both locations
    /// </summary>
    Auto,

    /// <summary>
    /// Only the default location is used
    /// </summary>
    Default,

    /// <summary>
    /// Only the VirtualStore location is used
    /// </summary>
    VirtualStore,
}