namespace RayCarrot.RCP.Metro;

public static class GameInstallationExtensions
{
    /// <summary>
    /// Gets the localized or custom display name for the game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the display name for</param>
    /// <returns>The display name</returns>
    public static LocalizedString GetDisplayName(this GameInstallation gameInstallation)
    {
        string? customName = gameInstallation.GetValue<string>(GameDataKey.RCP_CustomName);
        return customName != null
            ? new ConstLocString(customName)
            : gameInstallation.GameDescriptor.DisplayName;
    }
}