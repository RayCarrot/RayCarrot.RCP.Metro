namespace RayCarrot.RCP.Metro;

/// <summary>
/// The types of messages to display
/// </summary>
public enum MessageType
{
    /// <summary>
    /// A generic message
    /// </summary>
    Generic,

    /// <summary>
    /// An informative message
    /// </summary>
    Information,

    /// <summary>
    /// An error message
    /// </summary>
    Error,

    /// <summary>
    /// A warning message
    /// </summary>
    Warning,

    /// <summary>
    /// A success message
    /// </summary>
    Success,

    /// <summary>
    /// A message as a question
    /// </summary>
    Question
}