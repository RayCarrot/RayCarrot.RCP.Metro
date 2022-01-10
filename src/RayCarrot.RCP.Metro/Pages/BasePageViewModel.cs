using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

public abstract class BasePageViewModel : BaseRCPViewModel
{
    protected BasePageViewModel()
    {
        App.SelectedPageChanged += App_SelectedPageChangedAsync;
        _pageChangedLock = new AsyncLock();
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool _hasInitialized;
    private AsyncLock _pageChangedLock;

    public abstract AppPage Page { get; }

    private async void App_SelectedPageChangedAsync(object sender, PropertyChangedEventArgs<AppPage> e)
    {
        using (await _pageChangedLock.LockAsync())
        {
            if (e.NewValue != Page)
                return;

            await NavigatedToAsync();

            if (_hasInitialized)
                return;

            _hasInitialized = true;

            await InitializeAsync();

            Logger.Info("Initialized {0} page", Page);
        }
    }

    protected virtual Task NavigatedToAsync() => Task.CompletedTask;
    protected abstract Task InitializeAsync();
}