using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 translation utility
/// </summary>
public class Utility_Rayman2_Translation : Utility<Utility_Rayman2_Translation_Control, Utility_Rayman2_Translation_ViewModel>
{
    public override string DisplayHeader => Resources.R2U_TranslationsHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_Rayman2_Translation;
    public override string InfoText => Resources.R2U_TranslationsInfo;
    public override bool RequiresAdditionalFiles => true;
    public override bool RequiresAdmin => !Services.File.CheckFileWriteAccess(ViewModel.GetFixSnaFilePath());
    public override bool IsAvailable => ViewModel.InstallDir.DirectoryExists && ViewModel.GetFixSnaFilePath().FileExists && ViewModel.GetTexturesCntFilePath().FileExists;
    public override IEnumerable<string> GetAppliedUtilities()
    {
        Utility_Rayman2_Translation_ViewModel.Rayman2Translation? translation = ViewModel.GetAppliedRayman2Translation();

        if (translation != Utility_Rayman2_Translation_ViewModel.Rayman2Translation.Original && translation != null)
            yield return Resources.R2U_TranslationsHeader;
    }
}