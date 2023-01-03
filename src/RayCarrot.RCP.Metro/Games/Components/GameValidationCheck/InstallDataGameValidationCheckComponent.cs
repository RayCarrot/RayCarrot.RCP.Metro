﻿using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class InstallDataGameValidationCheckComponent : GameValidationCheckComponent
{
    public override bool IsValid()
    {
        // TODO-14: Log
        if (GameInstallation.GetObject<RCPGameInstallData>(GameDataKey.RCP_GameInstallData) is { } installData)
        {
            // Make sure the install directory exists
            if (!installData.InstallDir.DirectoryExists)
                return false;

            // Make sure the install mode is valid
            if (!Enum.IsDefined(typeof(RCPGameInstallData.RCPInstallMode), installData.InstallMode))
                return false;
        }

        return true;
    }
}