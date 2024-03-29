﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Designer create config utility
/// </summary>
public class Utility_RaymanDesigner_CreateConfig : Utility<Utility_RaymanDesigner_CreateConfig_Control, Utility_RaymanDesigner_CreateConfig_ViewModel>
{
    public Utility_RaymanDesigner_CreateConfig(GameInstallation gameInstallation) 
        : base(new Utility_RaymanDesigner_CreateConfig_ViewModel(gameInstallation))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.RDU_CreateConfigHeader));
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanDesigner_CreateConfig;
    public override LocalizedString InfoText => new ResourceLocString(nameof(Resources.RDU_CreateConfigInfo));
    public override bool RequiresAdmin => ViewModel.ConfigPath.FileExists && !Services.File.CheckFileWriteAccess(ViewModel.ConfigPath);
    public override bool IsAvailable => GameInstallation.InstallLocation.Directory.DirectoryExists;
}