namespace RayCarrot.RCP.Metro;

public class MainWindowViewModel : BaseViewModel
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
        App = app;
        GamesPage = gamesPage;
        ProgressionPage = progressionPage;
        UtilitiesPage = utilitiesPage;
        ModsPage = modsPage;
        SettingsPage = settingsPage;
        AboutPage = aboutPage;
        DebugPage = debugPage;

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
}