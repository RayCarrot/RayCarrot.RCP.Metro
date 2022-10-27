﻿#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 (DOSBox) game manager
/// </summary>
public sealed class GameManager_Rayman1_DOSBox : GameManager_DOSBox
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Rayman1;

    /// <summary>
    /// The executable name for the game. This is independent of the <see cref="GameDescriptor.DefaultFileName"/> which is used to launch the game.
    /// </summary>
    public override string ExecutableName => "RAYMAN.EXE";

    /// <summary>
    /// The Rayman Forever folder name, if available
    /// </summary>
    public override string RaymanForeverFolderName => "Rayman";

    /// <summary>
    /// The DOSBox file path
    /// </summary>
    public override FileSystemPath DOSBoxFilePath => Services.Data.Utility_TPLSData?.IsEnabled != true ? base.DOSBoxFilePath : Services.Data.Utility_TPLSData.DOSBoxFilePath;

    /// <summary>
    /// Optional additional config files
    /// </summary>
    public override IEnumerable<FileSystemPath> AdditionalConfigFiles => Services.Data.Utility_TPLSData?.IsEnabled != true ? base.AdditionalConfigFiles : new FileSystemPath[]
    {
        Services.Data.Utility_TPLSData.ConfigFilePath
    };
    #endregion
}