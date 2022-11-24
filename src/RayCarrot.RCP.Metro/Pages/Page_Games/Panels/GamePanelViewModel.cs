using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

public abstract class GamePanelViewModel : BaseViewModel
{
    #region Constructor

    protected GamePanelViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    #endregion

    #region Private Fields

    private bool _hasLoaded;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public GameDescriptor GameDescriptor => GameInstallation.GameDescriptor;

    public abstract GenericIconKind Icon { get; }
    public abstract LocalizedString Header { get; }
    public virtual bool CanRefresh => false;

    public bool IsLoading { get; private set; }

    #endregion

    #region Protected Methods

    protected abstract Task LoadAsyncImpl();

    #endregion

    #region Public Methods

    public async Task RefreshAsync()
    {
        if (!_hasLoaded)
            return;

        _hasLoaded = false;
        await LoadAsync();
    }

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