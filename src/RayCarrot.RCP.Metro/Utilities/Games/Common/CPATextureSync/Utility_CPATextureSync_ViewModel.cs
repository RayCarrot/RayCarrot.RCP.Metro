using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

// TODO-14: Maybe we need to have a general utility as well for other games like Donald Duck - unless we also add those to RCP?

public class Utility_CPATextureSync_ViewModel : BaseViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation</param>
    /// <param name="data">The game texture sync data</param>
    public Utility_CPATextureSync_ViewModel(GameInstallation gameInstallation, CPATextureSyncData data)
    {
        // Set properties
        TextureSyncManager = new CPATextureSyncManager(gameInstallation, data);

        // Create commands
        SyncTextureInfoCommand = new AsyncRelayCommand(SyncTextureInfoAsync);
    }

    #endregion

    #region Commands

    public ICommand SyncTextureInfoCommand { get; }

    #endregion

    #region Private Properties

    private CPATextureSyncManager TextureSyncManager { get; }

    #endregion

    #region Public Properties

    public bool IsLoading { get; set; }

    #endregion

    #region Public Methods

    public async Task SyncTextureInfoAsync()
    {
        try
        {
            IsLoading = true;
            await TextureSyncManager.SyncTextureInfoAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}