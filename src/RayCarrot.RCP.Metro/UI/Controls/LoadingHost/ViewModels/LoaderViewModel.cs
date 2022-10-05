using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Metro;

public class LoaderViewModel : BaseViewModel
{
    public LoaderViewModel()
    {
        CancellationTokenSource = new CancellationTokenSource();
        LoadLock = new AsyncLock();

        CancelCommand = new RelayCommand(Cancel);
    }

    private LoadStateViewModel? _state;

    private AsyncLock LoadLock { get; }
    private CancellationTokenSource CancellationTokenSource { get; }

    public ICommand CancelCommand { get; }

    public LoadStateViewModel? State
    {
        get => _state;
        private set
        {
            _state = value;
            OnIsRunningChanged();
        }
    }

    public bool IsRunning => State != null;

    public void Cancel()
    {
        CancellationTokenSource.Cancel();
    }

    public Task<LoadState> RunAsync() => RunAsync(null);

    public async Task<LoadState> RunAsync(string? status)
    {
        // Await the lock and get the disposable
        IDisposable d = await LoadLock.LockAsync();

        // TODO-UPDATE: If state is not null we can log warning since prev operation was not correctly disposed

        // Create a new state view model
        State = new LoadStateViewModel()
        {
            Status = status
        };

        // Return the load state
        return new LoadState(State, CancellationTokenSource.Token, () =>
        {
            d.Dispose();
            State = null;
        });
    }

    public event EventHandler? IsRunningChanged;

    protected virtual void OnIsRunningChanged() => IsRunningChanged?.Invoke(this, EventArgs.Empty);
}