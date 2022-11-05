﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Fiesta Run save fix utility
/// </summary>
public class Utility_RaymanFiestaRun_SaveFix : Utility<Utility_RaymanFiestaRun_SaveFix_Control, Utility_RaymanFiestaRun_SaveFix_ViewModel>
{
    public Utility_RaymanFiestaRun_SaveFix(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation, int slotIndex) 
        : base(new Utility_RaymanFiestaRun_SaveFix_ViewModel(gameDescriptor, gameInstallation, slotIndex))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override string DisplayHeader => Resources.RFRU_SaveFixHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanFiestaRun_SaveFix;
    public override string InfoText => Resources.RFRU_SaveFixInfo;
    public override bool IsAvailable => ViewModel.SaveFilePath.FileExists;
}