﻿namespace RayCarrot.RCP.Metro.Games.Tools.PerLevelSoundtrack;

public class PerLevelSoundtrackInstallableTool : InstallableTool
{
    public override string ToolId => "PerLevelSoundtrack";
    public override Version LatestVersion => new(3, 1, 0);

    // TODO-UPDATE: Replace url with RCP one
    public override Uri DownloadUri => new("https://github.com/PluMGMK/rayman-tpls-tsr/releases/download/v3.1/rayman-tpls-tsr.zip");

    public FileSystemPath CueFilePath => InstallDirectory + "TPLSTSR4.cue";

    public override void OnUninstalled()
    {
        foreach (GameInstallation gameInstallation in Services.Games.GetInstalledGames())
        {
            if (gameInstallation.GetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData) != null)
            {
                gameInstallation.SetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData, null);
                Services.Messenger.Send(new ModifiedGamesMessage(gameInstallation));
            }
        }
    }
}