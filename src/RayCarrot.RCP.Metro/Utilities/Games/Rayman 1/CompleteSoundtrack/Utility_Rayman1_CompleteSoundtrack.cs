using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

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

    public override string DisplayHeader => Resources.R1U_CompleteOSTHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_Rayman1_CompleteSoundtrack;
    public override string InfoText => Resources.R1U_CompleteOSTInfo;
    public override bool RequiresAdditionalFiles => true;
    public override bool RequiresAdmin => !Services.File.CheckDirectoryWriteAccess(ViewModel.MusicDir);
    public override bool IsAvailable => ViewModel.CanMusicBeReplaced;
    public override IEnumerable<string> GetAppliedUtilities()
    {
        if (ViewModel.GetIsOriginalSoundtrack() == false)
            yield return Resources.R1U_CompleteOSTHeader;
    }
}