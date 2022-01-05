#nullable disable
using System;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A view model for the IPK Archive Explorer utility
/// </summary>
public class Utility_ArchiveExplorer_IPK_ViewModel : Utility_BaseArchiveExplorer_ViewModel<GameMode>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_ArchiveExplorer_IPK_ViewModel()
    {
        GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.RaymanOriginsPC, new GameMode[]
        {
            GameMode.RaymanOriginsPC,
            GameMode.RaymanOriginsPS3,
            GameMode.RaymanOriginsXbox360,
            GameMode.RaymanOriginsWii,
            GameMode.RaymanOriginsPSVita,
            GameMode.RaymanLegendsPC,
            GameMode.RaymanLegendsXbox360,
            GameMode.RaymanLegendsWiiU,
            GameMode.RaymanLegendsPSVita,
            GameMode.RaymanLegendsPS4,
            GameMode.RaymanLegendsSwitch,
            GameMode.RaymanAdventuresAndroid,
            GameMode.RaymanAdventuresiOS,
            GameMode.RaymanMiniMac,
            GameMode.JustDance2017WiiU,
            GameMode.ChildOfLightPC,
            GameMode.ChildOfLightPSVita,
            GameMode.ValiantHeartsAndroid,
            GameMode.GravityFalls3DS,
        });
    }

    /// <summary>
    /// Gets a new archive data manager
    /// </summary>
    /// <param name="mode">The archive mode</param>
    /// <returns>The archive data manager</returns>
    protected override IArchiveDataManager GetArchiveDataManager(ArchiveMode mode)
    {
        var attr = GameModeSelection.SelectedValue.GetAttribute<UbiArtGameModeInfoAttribute>();
        var settings = UbiArtSettings.GetDefaultSettings(attr.Game, attr.Platform);

        throw new NotImplementedException();
        //return new UbiArtIPKArchiveDataManager(settings, 
        //    mode == ArchiveMode.Explorer 
        //        ? UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed 
        //        : UbiArtIPKArchiveConfigViewModel.FileCompressionMode.MatchesSetting);
    }

    /// <summary>
    /// The file extension for the archive
    /// </summary>
    public override string ArchiveFileExtension => ".ipk";

    /// <summary>
    /// The game mode selection
    /// </summary>
    public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }
}