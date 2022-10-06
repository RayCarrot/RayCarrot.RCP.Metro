using System;
using System.Threading;

namespace RayCarrot.RCP.Metro;

public class LoadState : IDisposable
{
    public LoadState(LoadStateViewModel viewModel, CancellationToken cancellationToken, Action disposeAction)
    {
        ViewModel = viewModel;
        CancellationToken = cancellationToken;
        DisposeAction = disposeAction;
    }

    private LoadStateViewModel ViewModel { get; }
    private Action DisposeAction { get; }

    public CancellationToken CancellationToken { get; }

    public void SetStatus(string? status) => ViewModel.Status = status;
    public void SetProgress(Progress progress) => ViewModel.SetProgress(progress);
    public void SetCanCancel(bool canCanel) => ViewModel.CanCancel = canCanel;

    public void Dispose()
    {
        DisposeAction();
    }
}