namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public abstract class DownloadableModViewModel : BaseViewModel, IDisposable
{
    public virtual void OnSelected() { }
    public virtual void Dispose() { }
}