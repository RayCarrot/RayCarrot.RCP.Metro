using System.ComponentModel;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public abstract class ConfigPageViewModel : GameOptionsDialogPageViewModel
{
    protected ConfigPageViewModel() 
    {
        GraphicsMode = new GraphicsModeSelectionViewModel();
        ConfigLocations = new ObservableCollection<LinkItemViewModel>();

        ConfigLocations.EnableCollectionSynchronization();

        GraphicsMode.GraphicsModeChanged += GraphicsMode_GraphicsModeChanged;
    }

    public override LocalizedString PageName => new ResourceLocString(nameof(Resources.GameOptions_Config));
    public override GenericIconKind PageIcon => GenericIconKind.GameOptions_Config;

    /// <summary>
    /// Indicates if the page can be saved
    /// </summary>
    public override bool CanSave => true;

    /// <summary>
    /// The graphics mode for the game, such as the resolution
    /// </summary>
    public GraphicsModeSelectionViewModel GraphicsMode { get; }

    public ObservableCollection<LinkItemViewModel> ConfigLocations { get; }

    private void ConfigPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        ConfigPropertyChanged(e.PropertyName);
    }

    private void GraphicsMode_GraphicsModeChanged(object sender, EventArgs e)
    {
        UnsavedChanges = true;
    }

    protected virtual void ConfigPropertyChanged(string propertyName) { }

    protected void AddConfigLocation(LinkItemViewModel.LinkType type, string linkPath)
    {
        LinkItemViewModel link = new(type, linkPath);

        if (link.IsValid)
            ConfigLocations.Add(link);
    }

    protected override Task PreLoadAsync()
    {
        PropertyChanged -= ConfigPageViewModel_PropertyChanged;
        ConfigLocations.Clear();
        return Task.CompletedTask;
    }

    protected override Task PostLoadAsync()
    {
        PropertyChanged += ConfigPageViewModel_PropertyChanged;

        // Load icons
        return Task.Run(() =>
        {
            foreach (LinkItemViewModel link in ConfigLocations)
                link.LoadIcon();
        });
    }

    public override void Dispose()
    {
        base.Dispose();

        GraphicsMode.GraphicsModeChanged -= GraphicsMode_GraphicsModeChanged;
    }
}