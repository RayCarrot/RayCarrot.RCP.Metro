using System;

namespace RayCarrot.RCP.Metro;

public class MainWindowViewModel : BaseViewModel, IDisposable
{
    public MainWindowViewModel(
        AppViewModel app,
        Page_Games_ViewModel gamesPage, 
        Page_Progression_ViewModel progressionPage, 
        Page_Utilities_ViewModel utilitiesPage, 
        Page_Mods_ViewModel modsPage, 
        Page_Settings_ViewModel settingsPage, 
        Page_About_ViewModel aboutPage, 
        Page_Debug_ViewModel debugPage)
    {
        App = app ?? throw new ArgumentNullException(nameof(app));
        GamesPage = gamesPage ?? throw new ArgumentNullException(nameof(gamesPage));
        ProgressionPage = progressionPage ?? throw new ArgumentNullException(nameof(progressionPage));
        UtilitiesPage = utilitiesPage ?? throw new ArgumentNullException(nameof(utilitiesPage));
        ModsPage = modsPage ?? throw new ArgumentNullException(nameof(modsPage));
        SettingsPage = settingsPage ?? throw new ArgumentNullException(nameof(settingsPage));
        AboutPage = aboutPage ?? throw new ArgumentNullException(nameof(aboutPage));
        DebugPage = debugPage ?? throw new ArgumentNullException(nameof(debugPage));

        AppTitle = App.IsRunningAsAdmin ? Resources.AppNameAdmin : Resources.AppName;
    }

    public AppViewModel App { get; } // TODO: Remove this from here
    public Page_Games_ViewModel GamesPage { get; }
    public Page_Progression_ViewModel ProgressionPage { get; }
    public Page_Utilities_ViewModel UtilitiesPage { get; }
    public Page_Mods_ViewModel ModsPage { get; }
    public Page_Settings_ViewModel SettingsPage { get; }
    public Page_About_ViewModel AboutPage { get; }
    public Page_Debug_ViewModel DebugPage { get; }

    /// <summary>
    /// The title of the application
    /// </summary>
    public string AppTitle { get; }

    public void Dispose()
    {
        GamesPage.Dispose();
        UtilitiesPage.Dispose();
        ModsPage.Dispose();
    }
}