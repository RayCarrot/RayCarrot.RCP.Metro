﻿using System.IO;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanByHisFans : ProgressionGameViewModel_RaymanDesigner
{
    public ProgressionGameViewModel_RaymanByHisFans(GameInstallation gameInstallation) : base(gameInstallation) { }

    protected override int LevelsCount => 40;
    protected override Ray1EngineVersion EngineVersion => Ray1EngineVersion.PC_Fan;

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // NOTE: Due to a mistake where the .sct files were not included in previous backups we need to keep this version for legacy support
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.cfg", "1", 0),
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.cfg", "0", 1),
        new(GameInstallation.InstallLocation + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 1),
    };
}