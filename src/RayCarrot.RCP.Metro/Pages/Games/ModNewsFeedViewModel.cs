using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.Pages.Games;

public class ModNewsFeedViewModel : BaseViewModel
{
    #region Constructor

    public ModNewsFeedViewModel(GamesManager games)
    {
        Games = games ?? throw new ArgumentNullException(nameof(games));
        
        Mods = new ObservableCollection<NewModViewModel>();
        Mods.EnableCollectionSynchronization();
    }

    #endregion

    #region Services

    private GamesManager Games { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<NewModViewModel> Mods { get; }
    public bool IsLoading { get; set; }

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            Mods.Clear();

            foreach (DownloadableModsSource source in DownloadableModsSource.GetSources())
            {
                await foreach (NewModViewModel modViewModel in source.GetNewModsAsync(Games))
                {
                    int insertIndex = Mods.SortedBinarySearch(modViewModel, NewModViewModelComparer.Instance);
                    if (insertIndex < 0)
                        insertIndex = ~insertIndex;
                    Mods.Insert(insertIndex, modViewModel);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Classes

    private class NewModViewModelComparer : IComparer<NewModViewModel>
    {
        public static readonly NewModViewModelComparer Instance = new();

        public int Compare(NewModViewModel? x, NewModViewModel? y)
        {
            if (ReferenceEquals(x, y)) 
                return 0;
            if (ReferenceEquals(null, y)) 
                return 1;
            if (ReferenceEquals(null, x)) 
                return -1;
            
            int modificationDateComparison = x.ModificationDate.CompareTo(y.ModificationDate);
            if (modificationDateComparison != 0) 
                return modificationDateComparison * -1;
            
            return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }

    #endregion
}