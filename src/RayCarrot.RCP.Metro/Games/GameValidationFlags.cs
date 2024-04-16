namespace RayCarrot.RCP.Metro;

[Flags]
public enum GameValidationFlags
{
    None = 0,
    
    /// <summary>
    /// Validates that the location exists and is valid. This is a fast operation as it
    /// mainly checks if file and directory paths exist.
    /// </summary>
    Location = 1 << 0,

    /// <summary>
    /// Validates that the location has a valid layout. This is a slow operation as it
    /// might require reading and parsing game files.
    /// </summary>
    Layout = 1 << 1,

    All = Location | Layout,
}