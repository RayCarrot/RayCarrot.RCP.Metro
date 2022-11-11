using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public abstract class GamePanelViewModel : BaseViewModel
{
    #region Constructor

    protected GamePanelViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
    }

    #endregion

    #region Private Fields

    private bool _hasLoaded;

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }

    public abstract GenericIconKind Icon { get; }
    public abstract LocalizedString Header { get; }

    public bool IsLoading { get; private set; }

    #endregion

    #region Protected Methods

    protected abstract Task LoadAsyncImpl();

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        if (IsLoading || _hasLoaded)
            return;

        try
        {
            IsLoading = true;

            await LoadAsyncImpl();

            _hasLoaded = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}