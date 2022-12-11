using ControlzEx.Theming;
using System.Windows;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for the <see cref="Application"/>
/// </summary>
public static class ApplicationExtensions
{
    private static void SetLocalThemes(Application app, string colorScheme)
    {
        // TODO: Find better way of doing this. Perhaps add our custom colors to the theme manager?

        static string GetThemeStyleSource(string colorScheme) => $"{AppViewModel.WPFApplicationBasePath}/UI/Brushes/Brushes.{colorScheme}.xaml";

        // Get previous source
        string lightSource = GetThemeStyleSource(ThemeManager.BaseColorLight);
        string darkSource = GetThemeStyleSource(ThemeManager.BaseColorDark);

        // Remove old source
        foreach (ResourceDictionary resourceDictionary in app.Resources.MergedDictionaries.ToArray())
        {
            if (String.Equals(resourceDictionary.Source?.ToString(), lightSource, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(resourceDictionary.Source?.ToString(), darkSource, StringComparison.OrdinalIgnoreCase))
                app.Resources.MergedDictionaries.Remove(resourceDictionary);
        }

        // Add new source
        app.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(GetThemeStyleSource(colorScheme))
        });
    }

    /// <summary>
    /// Sets the application theme
    /// </summary>
    /// <param name="app">The application</param>
    /// <param name="darkMode">True to use dark mode or false to use light mode</param>
    /// <param name="sync">Indicates if the theme should sync with the system theme. This will override the <see cref="darkMode"/> setting.</param>
    /// <param name="color">Optional accent color</param>
    public static void SetTheme(this Application app, bool darkMode, bool sync, Color? color = null)
    {
        if (sync)
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);
        }
        else
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.DoNotSync;

            if (color == null)
            {
                // IDEA: Change the default color? I tried changing it to some Material Design colors for consistency, but
                //       none of them worked that well. It also looks a bit better having the app color be separate from
                //       the other colors in the app.
                ThemeManager.Current.ChangeTheme(app, $"{(darkMode ? "Dark" : "Light")}.Purple");
            }
            else
            {
                Theme customTheme = new(
                    name: "RCP",
                    displayName: "RCP",
                    baseColorScheme: darkMode ? "Dark" : "Light",
                    colorScheme: color.Value.ToString(),
                    primaryAccentColor: color.Value,
                    showcaseBrush: new SolidColorBrush(color.Value),
                    isRuntimeGenerated: true,
                    isHighContrast: false);

                ThemeManager.Current.ChangeTheme(app, customTheme);
            }
        }

        Theme? theme = ThemeManager.Current.DetectTheme(app);

        if (theme != null)
            SetLocalThemes(app, theme.BaseColorScheme);
    }
}