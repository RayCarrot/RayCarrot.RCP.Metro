#nullable disable
using GongSolutions.Wpf.DragDrop;
using NLog;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// The drop handler for a <see cref="PatchViewModel"/> collection
/// </summary>
public class PatchDropHandler : DefaultDropHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The options view model
    /// </summary>
    public PatchContainerViewModel ViewModel { get; set; }

    public override void Drop(IDropInfo dropInfo)
    {
        // Call base drop handler
        base.Drop(dropInfo);
            
        Logger.Debug("The patches for {0} have been reordered", ViewModel.DisplayName);

        ViewModel.RefreshPatchedFiles();
        ViewModel.HasChanges = true;
    }
}