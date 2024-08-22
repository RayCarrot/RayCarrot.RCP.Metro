using Microsoft.WindowsAPICodePack.Taskbar;

namespace RayCarrot.RCP.Metro;

// TODO: Find better way of setting taskbar state. Setting it from the view model like this isn't ideal.

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

    public LoadingHostState State { get; set; }

    private static void SetTaskbarState(TaskbarProgressBarState state)
    {
        if (Services.Data.UI_ShowProgressOnTaskBar || state == TaskbarProgressBarState.NoProgress)
            App.Current.Dispatcher?.Invoke(() => App.Current.MainWindow?.SetTaskbarProgressState(state));
    }

    private static void SetTaskbarProgress(Progress progress)
    {
        if (Services.Data.UI_ShowProgressOnTaskBar)
            App.Current.Dispatcher?.Invoke(() => App.Current.MainWindow?.SetTaskbarProgressValue(progress));
    }

    private static void FlashWindow()
    {
        if (Services.Data.UI_FlashWindowOnTaskBar)
            App.Current.Dispatcher?.Invoke(() => App.Current.MainWindow?.Flash(2, true));
    }

    public void Cancel()
    {
        if (CanCancel)
            CancellationTokenSource.Cancel();
    }

    public LoaderLoadState CreateState(Action disposeAction)
    {
        SetTaskbarState(TaskbarProgressBarState.Indeterminate);
        return new LoaderLoadState(this, CancellationTokenSource.Token, () =>
        {
            disposeAction();
            SetTaskbarState(TaskbarProgressBarState.NoProgress);
        });
    }

    public void SetProgress(Progress progress)
    {
        HasProgress = true;
        CurrentProgress = progress.Current;
        MinProgress = progress.Min;
        MaxProgress = progress.Max;

        SetTaskbarProgress(progress);
    }

    public void Error()
    {
        CanCancel = false;
        State = LoadingHostState.Error;

        SetTaskbarState(TaskbarProgressBarState.Error);
        FlashWindow();
    }

    public void Complete()
    {
        SetProgress(Progress.Complete);
        CanCancel = false;
        State = LoadingHostState.Completed;

        SetTaskbarState(TaskbarProgressBarState.NoProgress);
        FlashWindow();
    }
}