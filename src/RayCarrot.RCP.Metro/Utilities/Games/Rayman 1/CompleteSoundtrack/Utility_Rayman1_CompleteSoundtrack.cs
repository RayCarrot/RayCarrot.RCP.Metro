using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 complete soundtrack utility
/// </summary>
public class Utility_Rayman1_CompleteSoundtrack : Utility<Utility_Rayman1_CompleteSoundtrack_UI, Utility_Rayman1_CompleteSoundtrack_ViewModel>
{
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