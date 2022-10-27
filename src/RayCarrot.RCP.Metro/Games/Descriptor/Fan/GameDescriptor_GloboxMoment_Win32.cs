#nullable disable
using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Globox Moment game descriptor
/// </summary>
public sealed class GameDescriptor_GloboxMoment_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "GloboxMoment_Win32";
    public override Game Game => Game.GloboxMoment;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.GloboxMoment;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Fan;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Globox Moment";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Globox Moment";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Globox Moment.exe";

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => false;

    /// <summary>
    /// The files to remove when uninstalling
    /// </summary>
    public override IEnumerable<FileSystemPath> UninstallFiles => new FileSystemPath[]
    {
        Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications" + "globoxmoment.ini"
    };

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_GloboxMoment(gameInstallation).Yield();

    #endregion
}