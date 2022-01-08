namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Common paths in the Registry
    /// </summary>
    public static class CommonRegistryPaths
    {
        /// <summary>
        /// The path of the RegEdit settings key
        /// </summary>
        public const string RegeditSettingsPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";

        /// <summary>
        /// The path of the RegEdit favorites key
        /// </summary>
        public const string RegeditFavoritesPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit\Favorites";

        /// <summary>
        /// The path of the Uninstall/Change programs key
        /// </summary>
        public const string InstalledPrograms = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    }
}