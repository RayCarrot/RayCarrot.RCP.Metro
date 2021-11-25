#nullable disable
using System;

namespace RayCarrot.RCP.Metro;

public abstract class GameOptionsDialog_ConfigPageViewModel : GameOptionsDialog_BasePageViewModel
{
    protected GameOptionsDialog_ConfigPageViewModel() : base(new ResourceLocString(nameof(Resources.GameOptions_Config)), GenericIconKind.GameOptions_Config)
    {
        GraphicsMode = new GraphicsModeSelectionViewModel();
        GraphicsMode.GraphicsModeChanged += GraphicsMode_GraphicsModeChanged;
    }

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