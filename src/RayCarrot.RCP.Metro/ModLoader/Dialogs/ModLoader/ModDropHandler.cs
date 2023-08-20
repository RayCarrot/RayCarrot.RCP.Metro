using GongSolutions.Wpf.DragDrop;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

/// <summary>
/// The drop handler for a <see cref="InstalledModViewModel"/> collection
/// </summary>
public class ModDropHandler : DefaultDropHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The options view model
    /// </summary>
    public ModLoaderViewModel? ViewModel { get; set; }

    public override void Drop(IDropInfo dropInfo)
    {
        // Call base drop handler
        base.Drop(dropInfo);
            
        Logger.Debug("The mods have been reordered");

        ViewModel?.OnReorderedMods();
    }
}