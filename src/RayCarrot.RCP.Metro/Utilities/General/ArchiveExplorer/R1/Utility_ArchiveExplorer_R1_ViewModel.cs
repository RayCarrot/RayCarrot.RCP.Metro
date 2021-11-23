using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A view model for the R1 Archive Explorer utility
/// </summary>
public class Utility_ArchiveExplorer_R1_ViewModel : Utility_BaseArchiveExplorer_ViewModel<GameMode>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_ArchiveExplorer_R1_ViewModel()
    {
        GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.RayKitPC, new GameMode[]
        {
            GameMode.RayEduPC,
            GameMode.RayKitPC,
            GameMode.RayFanPC,
            GameMode.Ray60nPC,
        });
    }

    /// <summary>
    /// Gets a new archive data manager
    /// </summary>
    /// <param name="mode">The archive mode</param>
    /// <returns>The archive data manager</returns>
    protected override IArchiveDataManager GetArchiveDataManager(ArchiveMode mode)
    {
        // Get the settings
        var attr = GameModeSelection.SelectedValue.GetAttribute<Ray1GameModeInfoAttribute>();
        var gameSettings = Ray1Settings.GetDefaultSettings(attr.Game, attr.Platform);

        return new Ray1PCArchiveDataManager(new Ray1PCArchiveConfigViewModel(gameSettings));
    }

    /// <summary>
    /// The file extension for the archive
    /// </summary>
    public override string ArchiveFileExtension => ".dat";

    /// <summary>
    /// The game mode selection
    /// </summary>
    public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }
}