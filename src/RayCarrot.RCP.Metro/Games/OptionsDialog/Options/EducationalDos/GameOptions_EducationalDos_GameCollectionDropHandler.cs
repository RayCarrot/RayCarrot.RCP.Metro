#nullable disable
using GongSolutions.Wpf.DragDrop;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The drop handler for <see cref="GameOptions_EducationalDos_Control"/> games collection
/// </summary>
public class GameOptions_EducationalDos_GameCollectionDropHandler : DefaultDropHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The options view model
    /// </summary>
    public GameOptions_EducationalDos_ViewModel ViewModel { get; set; }

    public override async void Drop(IDropInfo dropInfo)
    {
        // Call base drop handler
        base.Drop(dropInfo);
            
        Logger.Debug("The educational games have been reordered");

        // Save the new order
        await ViewModel.SaveAsync();
    }
}