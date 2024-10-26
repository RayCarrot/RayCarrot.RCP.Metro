namespace RayCarrot.RCP.Metro.Pages.Utilities;

/// <summary>
/// Base view model for a utility
/// </summary>
public abstract class UtilityViewModel : BaseViewModel, IDisposable
{
    #region Public Properties

    public abstract LocalizedString DisplayHeader { get; }
    public abstract GenericIconKind Icon { get; }

    public bool IsLoading { get; set; } // TODO: Show in UI

    #endregion

    #region Public Methods

    public virtual void Dispose()
    {
        DisplayHeader.Dispose();
    }

    #endregion
}