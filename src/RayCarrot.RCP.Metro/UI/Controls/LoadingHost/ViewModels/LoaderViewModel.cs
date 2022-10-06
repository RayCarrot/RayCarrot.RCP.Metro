using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Metro;

public class LoaderViewModel : BaseViewModel
{
    public LoaderViewModel()
    {
        LoadLock = new AsyncLock();

        CancelCommand = new RelayCommand(Cancel);
    }

    private LoadStateViewModel? _stateViewModel;

    private AsyncLock LoadLock { get; }

    public ICommand CancelCommand { get; }

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

    public void Cancel() => StateViewModel?.Cancel();

    public Task<LoadState> RunAsync() => RunAsync(null, false);

    public Task<LoadState> RunAsync(string? status) => RunAsync(status, false);

    public async Task<LoadState> RunAsync(string? status, bool canCancel)
    {
        // Await the lock and get the disposable
        IDisposable d = await LoadLock.LockAsync();

        // TODO-UPDATE: If state is not null we can log warning since prev operation was not correctly disposed

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

    public event EventHandler? IsRunningChanged;

    protected virtual void OnIsRunningChanged() => IsRunningChanged?.Invoke(this, EventArgs.Empty);
}