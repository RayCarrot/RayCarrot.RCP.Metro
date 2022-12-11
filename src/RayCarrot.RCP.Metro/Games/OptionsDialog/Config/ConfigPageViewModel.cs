using System;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public abstract class ConfigPageViewModel : GameOptionsDialogPageViewModel
{
    protected ConfigPageViewModel() 
    {
        GraphicsMode = new GraphicsModeSelectionViewModel();
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

    private void GraphicsMode_GraphicsModeChanged(object sender, EventArgs e)
    {
        UnsavedChanges = true;
    }

    public override void Dispose()
    {
        base.Dispose();

        GraphicsMode.GraphicsModeChanged -= GraphicsMode_GraphicsModeChanged;
    }
}