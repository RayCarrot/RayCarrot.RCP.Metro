#nullable disable
using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman The Dark Magician's Reign of Terror game info
/// </summary>
public sealed class GameInfo_TheDarkMagiciansReignofTerror : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.TheDarkMagiciansReignofTerror;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Fan;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman: The Dark Magician's Reign of Terror";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman The Dark Magicians Reign of Terror";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman! Dark Magician's reign of terror!.exe";

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => false;

    /// <summary>
    /// The directories to remove when uninstalling. This should not include the game install directory as that is included by default.
    /// </summary>
    public override IEnumerable<FileSystemPath> UninstallDirectories => new FileSystemPath[]
    {
        Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman__Dark_Magician_s_reign_of_terror_"
    };

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_TheDarkMagiciansReignofTerror(gameInstallation).Yield();

    #endregion
}