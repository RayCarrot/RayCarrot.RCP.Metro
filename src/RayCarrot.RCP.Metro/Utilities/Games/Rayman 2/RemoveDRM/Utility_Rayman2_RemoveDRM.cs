using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 DRM removal utility
/// </summary>
public class Utility_Rayman2_RemoveDRM : Utility<Utility_Rayman2_RemoveDRM_UI, Utility_Rayman2_RemoveDRM_ViewModel>
{
    public override string DisplayHeader => Resources.R2U_RemoveDRM_Header;
    public override GenericIconKind Icon => GenericIconKind.Utilities_Rayman2_RemoveDRM;
    public override string InfoText => Resources.R2U_RemoveDRM_Info;
    public override string WarningText => Resources.R2U_RemoveDRM_Warning;
    public override bool RequiresAdmin => !Services.File.CheckFileWriteAccess(ViewModel.SnaOffsets.Keys.FirstOrDefault());
    public override bool IsAvailable => ViewModel.SnaOffsets.Any();
    public override IEnumerable<string> GetAppliedUtilities()
    {
        if (ViewModel.HasBeenApplied)
            yield return Resources.R2U_RemoveDRM_Header;
    }
}