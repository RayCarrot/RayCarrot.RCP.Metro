namespace RayCarrot.RCP.Metro;

public class PartialProgressLoadState : ILoadState
{
    public PartialProgressLoadState(ILoadState primaryLoadState, Func<Progress, Progress> progressCallback)
    {
        _primaryLoadState = primaryLoadState;
        _progressCallback = progressCallback;
    }

    private readonly ILoadState _primaryLoadState;
    private readonly Func<Progress, Progress> _progressCallback;

    public CancellationToken CancellationToken => _primaryLoadState.CancellationToken;

    public void SetStatus(string? status) => _primaryLoadState.SetStatus(status);
    public void SetProgress(Progress progress) => _primaryLoadState.SetProgress(_progressCallback(progress));
    public void SetCanCancel(bool canCanel) => _primaryLoadState.SetCanCancel(canCanel);
}