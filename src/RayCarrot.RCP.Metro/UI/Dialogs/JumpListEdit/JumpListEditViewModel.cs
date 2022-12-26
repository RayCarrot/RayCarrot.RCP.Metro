namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for editing the jump list
/// </summary>
public class JumpListEditViewModel : UserInputViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public JumpListEditViewModel()
    {
        // Set the title
        Title = Resources.JumpListEditor_Header;

        _autoSort = Services.Data.App_AutoSortJumpList;

        // Create the collections
        var included = new List<JumpListItemViewModel>();
        NotIncluded = new ObservableCollection<JumpListItemViewModel>();

        // Get all jump list items
        foreach (GameInstallation gameInstallation in Services.Games.GetInstalledGames())
        {
            LaunchGameComponent? component = gameInstallation.GetComponent<LaunchGameComponent>();

            if (component == null)
                continue;

            foreach (JumpListItemViewModel item in component.GetJumpListItems())
            {
                if (Services.Data.App_JumpListItems.Any(x => x.ItemId == item.Id))
                    included.Add(item);
                else
                    NotIncluded.Add(item);
            }
        }

        // Order the included games
        Included = included.OrderBy(x => Services.Data.App_JumpListItems.FindIndex(j => j.ItemId == x.Id)).ToObservableCollection();
    }

    #endregion

    #region Private Fields

    private bool _autoSort;

    #endregion

    #region Public Properties

    /// <summary>
    /// The included items
    /// </summary>
    public ObservableCollection<JumpListItemViewModel> Included { get; set; }

    /// <summary>
    /// The not included items
    /// </summary>
    public ObservableCollection<JumpListItemViewModel> NotIncluded { get; }

    /// <summary>
    /// Indicates if the items should be automatically sorted
    /// </summary>
    public bool AutoSort
    {
        get => _autoSort;
        set
        {
            _autoSort = value;

            // Sort
            if (value)
                Included = new ObservableCollection<JumpListItemViewModel>(Included.OrderBy(x => x));
        }
    }

    #endregion

    #region Public Methods

    public async Task LoadIconsAsync()
    {
        await Task.Run(() =>
        {
            foreach (JumpListItemViewModel item in Included.Concat(NotIncluded).ToArray())
                item.SetIconImageSource();
        });
    }

    #endregion
}