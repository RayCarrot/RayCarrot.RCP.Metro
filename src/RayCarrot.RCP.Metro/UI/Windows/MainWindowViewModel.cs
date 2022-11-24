using System;
using PropertyChanged;

namespace RayCarrot.RCP.Metro;

public class MainWindowViewModel : BaseViewModel, IDisposable
{
    public MainWindowViewModel(
        AppViewModel app,
        Page_GamesNew_ViewModel gamesPageNew, 
        Page_Progression_ViewModel progressionPage, 
        Page_Utilities_ViewModel utilitiesPage, 
        Page_Mods_ViewModel modsPage, 
        Page_Settings_ViewModel settingsPage, 
        Page_About_ViewModel aboutPage, 
        Page_Debug_ViewModel debugPage)
    {
        App = app ?? throw new ArgumentNullException(nameof(app));
        GamesPageNew = gamesPageNew ?? throw new ArgumentNullException(nameof(gamesPageNew));
        ProgressionPage = progressionPage ?? throw new ArgumentNullException(nameof(progressionPage));
        UtilitiesPage = utilitiesPage ?? throw new ArgumentNullException(nameof(utilitiesPage));
        ModsPage = modsPage ?? throw new ArgumentNullException(nameof(modsPage));
        SettingsPage = settingsPage ?? throw new ArgumentNullException(nameof(settingsPage));
        AboutPage = aboutPage ?? throw new ArgumentNullException(nameof(aboutPage));
        DebugPage = debugPage ?? throw new ArgumentNullException(nameof(debugPage));

        AppTitle = App.IsRunningAsAdmin ? Resources.AppNameAdmin : Resources.AppName;

        SelectedPageChanged += MainWindowViewModel_SelectedPageChanged;

        SelectedPage = AppPage.Games;
    }

    private AppPage _selectedPage = AppPage.None;

    public AppViewModel App { get; } // TODO: Remove this from here
    public Page_GamesNew_ViewModel GamesPageNew { get; }
    public Page_Progression_ViewModel ProgressionPage { get; }
    public Page_Utilities_ViewModel UtilitiesPage { get; }
    public Page_Mods_ViewModel ModsPage { get; }
    public Page_Settings_ViewModel SettingsPage { get; }
    public Page_About_ViewModel AboutPage { get; }
    public Page_Debug_ViewModel DebugPage { get; }

    /// <summary>
    /// The currently selected page
    /// </summary>
    public AppPage SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (_selectedPage == value)
                return;

            var oldValue = _selectedPage;
            _selectedPage = value;
            OnSelectedPageChanged(new PropertyChangedEventArgs<AppPage>(oldValue, value));
        }
    }

    /// <summary>
    /// The title of the application
    /// </summary>
    public string AppTitle { get; }

    /// <summary>
    /// Occurs when the selected page changes
    /// </summary>
    public event EventHandler<PropertyChangedEventArgs<AppPage>> SelectedPageChanged = delegate { };

    private async void MainWindowViewModel_SelectedPageChanged(object sender, PropertyChangedEventArgs<AppPage> e)
    {
        BasePageViewModel vm = e.NewValue switch
        {
            AppPage.Games => GamesPageNew,
            AppPage.Progression => ProgressionPage,
            AppPage.Utilities => UtilitiesPage,
            AppPage.Mods => ModsPage,
            AppPage.Settings => SettingsPage,
            AppPage.About => AboutPage,
            AppPage.Debug => DebugPage,
            _ => throw new ArgumentOutOfRangeException()
        };

        await vm.OnPageSelectedAsync();
    }

    [SuppressPropertyChangedWarnings]
    protected void OnSelectedPageChanged(PropertyChangedEventArgs<AppPage> e)
    {
        SelectedPageChanged(this, e);
    }

    public void Dispose()
    {
        UtilitiesPage.Dispose();
        ModsPage.Dispose();
    }
}