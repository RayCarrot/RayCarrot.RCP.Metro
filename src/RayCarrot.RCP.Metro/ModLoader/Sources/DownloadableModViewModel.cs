namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public abstract class DownloadableModViewModel : BaseViewModel, IDisposable
{
    public virtual Task LoadAsync() => Task.CompletedTask;
    public virtual void Dispose() { }
}