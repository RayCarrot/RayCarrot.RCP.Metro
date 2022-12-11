#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a utility
/// </summary>
public class UtilityViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="utility">The utility</param>
    public UtilityViewModel(Utility utility)
    {
        Utility = utility;
        DisplayHeader = new GeneratedLocString(() => Utility.DisplayHeader);
        IconKind = utility.Icon;
        InfoText = new GeneratedLocString(() => Utility.InfoText);
        WarningText = new GeneratedLocString(() => Utility.WarningText);
        IsAvailable = utility.IsAvailable;
        IsLoading = utility.IsLoading;

        utility.IsLoadingChanged += (_, _) => IsLoading = utility.IsLoading;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the utility can not be run due to requiring the app to run as administrator
    /// </summary>
    public bool RequiresAdmin => !Services.App.IsRunningAsAdmin && Utility.RequiresAdmin;

    public bool IsAvailable { get; }

    public bool IsLoading { get; set; }

    /// <summary>
    /// The utility header
    /// </summary>
    public LocalizedString DisplayHeader { get; }

    public GenericIconKind IconKind { get; }

    /// <summary>
    /// The utility info
    /// </summary>
    public LocalizedString InfoText { get; }

    /// <summary>
    /// The utility warning
    /// </summary>
    public LocalizedString WarningText { get; }

    /// <summary>
    /// The utility
    /// </summary>
    public Utility Utility { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
    /// </summary>
    public void Dispose()
    {
        DisplayHeader?.Dispose();
        InfoText?.Dispose();
        WarningText?.Dispose();
    }

    #endregion
}