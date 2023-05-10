using System.Windows.Input;
using PropertyChanged;
using RayCarrot.RCP.Metro.Pages.About;
using RayCarrot.RCP.Metro.Pages.Debug;
using RayCarrot.RCP.Metro.Pages.Games;
using RayCarrot.RCP.Metro.Pages.Mods;
using RayCarrot.RCP.Metro.Pages.Progression;
using RayCarrot.RCP.Metro.Pages.Settings;
using RayCarrot.RCP.Metro.Pages.Utilities;

namespace RayCarrot.RCP.Metro;

public class MainWindowViewModel : BaseViewModel, IDisposable
{
    public MainWindowViewModel(
        AppViewModel app,
        GamesPageViewModel gamesPage, 
        ProgressionPageViewModel progressionPage, 
        UtilitiesPageViewModel utilitiesPage, 
        ModsPageViewModel modsPage, 
        SettingsPageViewModel settingsPage, 
        AboutPageViewModel aboutPage, 
        DebugPageViewModel debugPage)
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

        SelectedPageChanged += MainWindowViewModel_SelectedPageChanged;

        SelectedPage = AppPage.Games;

        OpenBetaSurveyCommand = new RelayCommand(() => App.OpenUrl("https://forms.gle/cwDPDmFyhPSXLi4q6"));
    }

    private AppPage _selectedPage = AppPage.None;

    // TODO-UPDATE: Remove this when the beta is finished
    public ICommand OpenBetaSurveyCommand { get; }

    public AppViewModel App { get; } // TODO: Remove this from here
    public GamesPageViewModel GamesPage { get; }
    public ProgressionPageViewModel ProgressionPage { get; }
    public UtilitiesPageViewModel UtilitiesPage { get; }
    public ModsPageViewModel ModsPage { get; }
    public SettingsPageViewModel SettingsPage { get; }
    public AboutPageViewModel AboutPage { get; }
    public DebugPageViewModel DebugPage { get; }

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
            AppPage.Games => GamesPage,
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