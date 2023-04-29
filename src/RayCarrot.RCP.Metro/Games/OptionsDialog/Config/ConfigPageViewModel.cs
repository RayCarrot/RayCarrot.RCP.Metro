using System.Windows;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public abstract class ConfigPageViewModel : GameOptionsDialogPageViewModel
{
    protected ConfigPageViewModel() 
    {
        GraphicsMode = new GraphicsModeSelectionViewModel();
        ConfigLocations = new ObservableCollection<LinkItemViewModel>();

        BindingOperations.EnableCollectionSynchronization(ConfigLocations, RCP.Metro.App.Current);

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

    protected void AddConfigLocation(LinkItemViewModel.LinkType type, string linkPath)
    {
        LinkItemViewModel link = new(type, linkPath);

        if (link.IsValid)
            ConfigLocations.Add(link);
    }

    protected override Task PreLoadAsync()
    {
        ConfigLocations.Clear();
        return Task.CompletedTask;
    }

    protected override Task PostLoadAsync()
    {
        // Load icons
        return Task.Run(() =>
        {
            foreach (LinkItemViewModel link in ConfigLocations)
                link.LoadIcon();
        });
    }

    private void GraphicsMode_GraphicsModeChanged(object sender, EventArgs e)
    {
        UnsavedChanges = true;
    }

    public override void Dispose()
    {
        base.Dispose();

        GraphicsMode.GraphicsModeChanged -= GraphicsMode_GraphicsModeChanged;

        BindingOperations.DisableCollectionSynchronization(ConfigLocations);
    }
}