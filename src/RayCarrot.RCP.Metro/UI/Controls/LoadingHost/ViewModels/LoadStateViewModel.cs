using Microsoft.WindowsAPICodePack.Taskbar;

namespace RayCarrot.RCP.Metro;

// TODO: Find better way of setting taskbar progress. Setting it from the view model like this isn't ideal.

public class LoadStateViewModel : BaseViewModel
{
    public LoadStateViewModel()
    {
        CancellationTokenSource = new CancellationTokenSource();
    }

    private CancellationTokenSource CancellationTokenSource { get; }

    public string? Status { get; set; }

    public bool HasProgress { get; set; }
    public double CurrentProgress { get; set; }
    public double MinProgress { get; set; }
    public double MaxProgress { get; set; }

    public bool CanCancel { get; set; }

    public void Cancel()
    {
        if (CanCancel)
            CancellationTokenSource.Cancel();
    }

    public LoadState CreateState(Action disposeAction) => new(this, CancellationTokenSource.Token, () =>
    {
        disposeAction();
        App.Current.Dispatcher?.Invoke(() => App.Current.MainWindow?.SetTaskbarProgressState(TaskbarProgressBarState.NoProgress));
    });

    public void SetProgress(Progress progress)
    {
        HasProgress = true;
        CurrentProgress = progress.Current;
        MinProgress = progress.Min;
        MaxProgress = progress.Max;

        // TODO-UPDATE: Respect setting which disables this
        App.Current.Dispatcher?.Invoke(() => App.Current.MainWindow?.SetTaskbarProgressValue(progress));
    }
}