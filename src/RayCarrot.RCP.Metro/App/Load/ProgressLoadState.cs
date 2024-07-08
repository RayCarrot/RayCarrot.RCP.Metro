namespace RayCarrot.RCP.Metro;

public class ProgressLoadState : ILoadState
{
    public ProgressLoadState(CancellationToken cancellationToken, Action<Progress> progressCallback)
    {
        CancellationToken = cancellationToken;
        _progressCallback = progressCallback;
    }

    private readonly Action<Progress> _progressCallback;

    public CancellationToken CancellationToken { get; }

    public void SetStatus(string? status) { }
    public void SetProgress(Progress progress) => _progressCallback(progress);
    public void SetCanCancel(bool canCanel) { }
}