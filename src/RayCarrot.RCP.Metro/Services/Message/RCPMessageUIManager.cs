﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The message UI manager for the Rayman Control Panel
/// </summary>
public class RCPMessageUIManager : IMessageUIManager
{
    public RCPMessageUIManager(AppUIManager appUi)
    {
        UI = appUi ?? throw new ArgumentNullException(nameof(appUi));
    }

    private AppUIManager UI { get; }

    /// <summary>
    /// Displays a message to the user
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="header">The header for the message</param>
    /// <param name="messageType">The type of message, determining its visual appearance</param>
    /// <param name="allowCancel">True if the option to cancel is present</param>
    /// <returns>True if the user accepted the message, otherwise false</returns>
    public Task<bool> DisplayMessageAsync(string message, string header, MessageType messageType, bool allowCancel)
    {
        return UI.DisplayMessageAsync(message, header, messageType, allowCancel, Array.Empty<DialogMessageActionViewModel>());
    }
}