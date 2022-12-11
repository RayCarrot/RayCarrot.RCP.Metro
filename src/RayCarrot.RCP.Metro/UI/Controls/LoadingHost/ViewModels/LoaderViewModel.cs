using System.Windows.Input;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Metro;

public class LoaderViewModel : BaseViewModel
{
    #region Constructor

    public LoaderViewModel()
    {
        _loadLock = new AsyncLock();

        CancelCommand = new RelayCommand(Cancel);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly AsyncLock _loadLock;
    private LoadStateViewModel? _stateViewModel;

    #endregion

    #region Commands

    public ICommand CancelCommand { get; }

    #endregion

    #region Public Properties

    public LoadStateViewModel? StateViewModel
    {
        get => _stateViewModel;
        private set
        {
            _stateViewModel = value;
            OnIsRunningChanged();
        }
    }

    public bool IsRunning => StateViewModel != null;

    #endregion

    #region Public Methods

    public void Cancel() => StateViewModel?.Cancel();

    public Task<LoadState> RunAsync() => RunAsync(null, false);

    public Task<LoadState> RunAsync(string? status) => RunAsync(status, false);

    public async Task<LoadState> RunAsync(string? status, bool canCancel)
    {
        // Await the lock and get the disposable
        IDisposable d = await _loadLock.LockAsync();

        if (StateViewModel != null)
            Logger.Warn("Starting a new loading operation while previous one did not finish correctly");

        // Create a new state view model
        StateViewModel = new LoadStateViewModel()
        {
            Status = status,
            CanCancel = canCancel,
        };

        // Return the load state
        return StateViewModel.CreateState(() =>
        {
            d.Dispose();
            StateViewModel = null;
        });
    }

    #endregion

    #region Events

    public event EventHandler? IsRunningChanged;

    protected virtual void OnIsRunningChanged() => IsRunningChanged?.Invoke(this, EventArgs.Empty);

    #endregion
}