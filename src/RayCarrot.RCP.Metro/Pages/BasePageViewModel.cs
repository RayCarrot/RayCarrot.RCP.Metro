using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

public abstract class BasePageViewModel : BaseRCPViewModel
{
    protected BasePageViewModel()
    {
        App.SelectedPageChanged += App_SelectedPageChangedAsync;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool _hasInitialized;

    public abstract AppPage Page { get; }

    private async void App_SelectedPageChangedAsync(object sender, PropertyChangedEventArgs<AppPage> e)
    {
        if (e.NewValue != Page)
            return;

        if (_hasInitialized)
            return;

        _hasInitialized = true;

        await InitializeAsync();

        Logger.Info("Initialized {0} page", Page);
    }

    protected abstract Task InitializeAsync();
}