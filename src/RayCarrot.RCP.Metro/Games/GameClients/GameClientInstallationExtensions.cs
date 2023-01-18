using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public static class GameClientInstallationExtensions
{
    /// <summary>
    /// Gets the localized or custom display name for the game client installation
    /// </summary>
    /// <param name="gameClientInstallation">The game client installation to get the display name for</param>
    /// <returns>The display name</returns>
    public static LocalizedString GetDisplayName(this GameClientInstallation gameClientInstallation)
    {
        string? customName = gameClientInstallation.GetValue<string>(GameClientDataKey.RCP_CustomName);
        return customName != null
            ? new ConstLocString(customName)
            : gameClientInstallation.GameClientDescriptor.DisplayName;
    }
}