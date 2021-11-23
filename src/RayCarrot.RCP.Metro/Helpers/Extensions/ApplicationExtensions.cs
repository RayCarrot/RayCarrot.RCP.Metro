using ControlzEx.Theming;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for the <see cref="Application"/>
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Sets the application theme
    /// </summary>
    /// <param name="app">The application</param>
    /// <param name="darkMode">True to use dark mode or false to use light mode</param>
    /// <param name="sync">Indicates if the theme should sync with the system theme. This will override the <see cref="darkMode"/> setting.</param>
    /// <param name="color">Optional accent color</param>
    public static void SetTheme(this Application app, bool darkMode, bool sync, string color = "Purple")
    {
        if (sync)
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);
        }
        else
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.DoNotSync;
            ThemeManager.Current.ChangeTheme(app, $"{(darkMode ? "Dark" : "Light")}.{color}");
        }
    }
}