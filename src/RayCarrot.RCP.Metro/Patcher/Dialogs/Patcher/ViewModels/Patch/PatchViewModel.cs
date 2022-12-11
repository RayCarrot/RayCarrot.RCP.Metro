using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro.Patcher;

public abstract class PatchViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    protected PatchViewModel(PatcherViewModel patcherViewModel)
    {
        PatcherViewModel = patcherViewModel;

        OpenWebsiteCommand = new RelayCommand(OpenWebsite);
    }

    #endregion

    #region Commands

    public ICommand OpenWebsiteCommand { get; }

    #endregion

    #region Public Properties

    public PatcherViewModel PatcherViewModel { get; }
    public abstract string ID { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Website { get; }
    public bool HasWebsite => Uri.TryCreate(Website, UriKind.Absolute, out _);
    public bool HasDescripton => !Description.IsNullOrWhiteSpace();
    public abstract ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }
    public ImageSource? Thumbnail { get; set; }

    #endregion

    #region Public Methods

    public void OpenWebsite()
    {
        Services.App.OpenUrl(Website);
    }

    public virtual void Dispose() { }

    #endregion
}