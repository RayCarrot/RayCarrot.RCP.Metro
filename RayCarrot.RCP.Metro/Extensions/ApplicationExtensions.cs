using System.Windows;
using MahApps.Metro;

namespace RayCarrot.RCP.Metro
{
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
        public static void SetTheme(this Application app, bool darkMode)
        {
            ThemeManager.ChangeAppTheme(app, $"Base{(darkMode ? "Dark" : "Light")}");
        }
    }
}