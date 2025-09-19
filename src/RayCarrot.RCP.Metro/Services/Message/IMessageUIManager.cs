namespace RayCarrot.RCP.Metro;

/// <summary>
/// A message UI Manager for managing message UI requests
/// </summary>
public interface IMessageUIManager
{
    /// <summary>
    /// Displays a message to the user
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="header">The header for the message</param>
    /// <param name="messageType">The type of message, determining its visual appearance</param>
    /// <param name="allowCancel">True if the option to cancel is present</param>
    /// <returns>True if the user accepted the message, otherwise false</returns>
    Task<bool> DisplayMessageAsync(string message, string header, MessageType messageType, bool allowCancel);
}