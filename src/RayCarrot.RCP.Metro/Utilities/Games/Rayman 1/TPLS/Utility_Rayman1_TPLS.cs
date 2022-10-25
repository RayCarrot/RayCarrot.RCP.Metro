using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 TPLS utility
/// </summary>
public class Utility_Rayman1_TPLS : Utility<Utility_Rayman1_TPLS_Control, Utility_Rayman1_TPLS_ViewModel>
{
    public Utility_Rayman1_TPLS(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override string DisplayHeader => Resources.R1U_TPLSHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_Rayman1_TPLS;
    public override string InfoText => Resources.R1U_TPLSInfo;
    public override bool RequiresAdditionalFiles => true;
    public override IEnumerable<string> GetAppliedUtilities()
    {
        if (Services.Data.Utility_TPLSData != null)
            yield return Resources.R1U_TPLSHeader;
    }
}