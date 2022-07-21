using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

public abstract class BasePageViewModel : BaseViewModel
{
    protected BasePageViewModel(AppViewModel app)
    {
        App = app ?? throw new ArgumentNullException(nameof(app));

        // TODO: This won't trigger for the initially selected page
        app.SelectedPageChanged += App_SelectedPageChangedAsync;
        _pageChangedLock = new AsyncLock();
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool _hasInitialized;
    private readonly AsyncLock _pageChangedLock;

    public AppViewModel App { get; }
    public abstract AppPage Page { get; }

    private async void App_SelectedPageChangedAsync(object sender, PropertyChangedEventArgs<AppPage> e)
    {
        if (e.NewValue != Page)
            return;

        await OnSelectedPageAsync();
    }

    private async Task OnSelectedPageAsync()
    {
        using (await _pageChangedLock.LockAsync())
        {
            await NavigatedToAsync();

            if (_hasInitialized)
                return;

            _hasInitialized = true;

            await InitializeAsync();

            Logger.Info("Initialized {0} page", Page);
        }
    }

    protected virtual Task NavigatedToAsync() => Task.CompletedTask;
    protected virtual Task InitializeAsync() => Task.CompletedTask;
}