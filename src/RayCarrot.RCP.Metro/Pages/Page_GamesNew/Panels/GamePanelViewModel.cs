using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public abstract class GamePanelViewModel : BaseViewModel
{
    #region Private Fields

    private bool _hasLoaded;

    #endregion

    #region Public Properties

    public abstract GenericIconKind Icon { get; }
    public abstract LocalizedString Header { get; }

    public bool IsLoading { get; private set; }

    #endregion

    #region Protected Methods

    protected abstract Task LoadAsyncImpl(GameInstallation gameInstallation);

    #endregion

    #region Public Methods

    public async Task LoadAsync(GameInstallation gameInstallation)
    {
        if (IsLoading || _hasLoaded)
            return;

        try
        {
            IsLoading = true;

            await LoadAsyncImpl(gameInstallation);

            _hasLoaded = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}