﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 complete soundtrack utility
/// </summary>
public class Utility_Rayman1_CompleteSoundtrack : Utility<Utility_Rayman1_CompleteSoundtrack_Control, Utility_Rayman1_CompleteSoundtrack_ViewModel>
{
    public Utility_Rayman1_CompleteSoundtrack(GameInstallation gameInstallation) 
        : base(new Utility_Rayman1_CompleteSoundtrack_ViewModel(gameInstallation))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.R1U_CompleteOSTHeader));
    public override GenericIconKind Icon => GenericIconKind.Utilities_Rayman1_CompleteSoundtrack;
    public override LocalizedString InfoText => new ResourceLocString(nameof(Resources.R1U_CompleteOSTInfo));
    public override bool RequiresAdditionalFiles => true;
    public override bool RequiresAdmin => ViewModel.MusicDir.DirectoryExists && !Services.File.CheckDirectoryWriteAccess(ViewModel.MusicDir);
    public override bool IsAvailable => ViewModel.CanMusicBeReplaced;
}