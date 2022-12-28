namespace RayCarrot.RCP.Metro.Games.Clients;

public abstract class GameClientGameConfigViewModel : BaseViewModel
{
    /// <summary>
    /// Indicates if there are any unsaved config changes
    /// </summary>
    public bool UnsavedChanges { get; set; }

    /// <summary>
    /// Indicates if the config can be saved
    /// </summary>
    public virtual bool CanSave => false;

    /// <summary>
    /// Indicates if the option to use recommended options in the config page is available
    /// </summary>
    public virtual bool CanUseRecommended => false;

    public abstract void Load();
    public virtual Task<bool> SaveAsync() => Task.FromResult(false);
    public virtual void UseRecommended() { }
}