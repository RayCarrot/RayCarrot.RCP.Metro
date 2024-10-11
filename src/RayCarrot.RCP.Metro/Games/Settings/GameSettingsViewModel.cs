using System.ComponentModel;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Games.Settings;

public abstract class GameSettingsViewModel : BaseViewModel
{
    #region Constructor

    protected GameSettingsViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        
        SettingsLocations = new ObservableCollection<LinkItemViewModel>();
        SettingsLocations.EnableCollectionSynchronization();

        ApplyRecommendedSettingsCommand = new RelayCommand(ApplyRecommendedSettings);
        SaveChangesCommand = new AsyncRelayCommand(SaveChangesAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ApplyRecommendedSettingsCommand { get; }
    public ICommand SaveChangesCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ObservableCollection<LinkItemViewModel> SettingsLocations { get; }

    public virtual bool HasRecommendedSettings => false;

    public bool IsLoading { get; set; }
    public bool UnsavedChanges { get; set; }

    #endregion

    #region Private Methods

    private void GameSettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        SettingsPropertyChanged(e.PropertyName);
    }

    #endregion

    #region Protected Methods

    protected void AddSettingsLocation(LinkItemViewModel.LinkType type, string linkPath)
    {
        LinkItemViewModel link = new(type, linkPath);

        if (link.IsValid)
            SettingsLocations.Add(link);
    }

    protected virtual void SettingsPropertyChanged(string propertyName) { }

    protected abstract Task LoadAsync();
    protected abstract Task SaveAsync();

    protected virtual void ApplyRecommendedSettings() { }

    #endregion

    #region Public Methods

    public async Task<bool> InitializeAsync()
    {
        PropertyChanged += GameSettingsViewModel_PropertyChanged;

        Logger.Info("Loading settings for {0}", GameInstallation.FullId);

        try
        {
            IsLoading = true;
            await Task.Run(LoadAsync);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading game settings");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameSettings_LoadError);

            return false;
        }
        finally
        {
            IsLoading = false;
        }

        Logger.Info("Finished loading settings");

        // Load icons
        await Task.Run(() =>
        {
            foreach (LinkItemViewModel link in SettingsLocations)
                link.LoadIcon();
        });

        return true;
    }

    public async Task SaveChangesAsync()
    {
        if (IsLoading)
            return;

        Logger.Info("Saving settings for {0}", GameInstallation.FullId);

        try
        {
            await SaveAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving settings for {0}", GameInstallation.FullId);

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameSettings_SaveError);

            return;
        }

        Logger.Info("Finished saving settings");

        UnsavedChanges = false;

        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);
    }

    #endregion
}