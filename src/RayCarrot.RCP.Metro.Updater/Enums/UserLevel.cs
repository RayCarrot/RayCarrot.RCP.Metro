namespace RayCarrot.RCP.Updater;

/// <summary>
/// Defines a level of access for the user using the application
/// </summary>
public enum UserLevel
{
    /// <summary>
    /// Normal feature, normally available for all users
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Advanced feature, available for slightly advanced users
    /// </summary>
    Advanced = 2,

    /// <summary>
    /// Pro feature, available for highly advanced users
    /// </summary>
    Technical = 3,

    /// <summary>
    /// Debug feature, available for developers and testers
    /// </summary>
    Debug = 4
}