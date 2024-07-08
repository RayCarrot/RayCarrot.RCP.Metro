namespace RayCarrot.RCP.Metro;

public interface ILoadState
{
    CancellationToken CancellationToken { get; }

    void SetStatus(string? status);
    void SetProgress(Progress progress);
    void SetCanCancel(bool canCanel);
}