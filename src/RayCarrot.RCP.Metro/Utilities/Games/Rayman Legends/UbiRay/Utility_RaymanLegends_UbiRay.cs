using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends UbiRay utility
/// </summary>
public class Utility_RaymanLegends_UbiRay : Utility<Utility_RaymanLegends_UbiRay_Control, Utility_RaymanLegends_UbiRay_ViewModel>
{
    public Utility_RaymanLegends_UbiRay(GameInstallation gameInstallation) 
        : base(new Utility_RaymanLegends_UbiRay_ViewModel(gameInstallation))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override string DisplayHeader => Resources.RLU_UbiRayHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanLegends_UbiRay;
    public override string InfoText => Resources.RLU_UbiRayInfo;
    public override bool RequiresAdmin => !Services.File.CheckFileWriteAccess(ViewModel.IPKFilePath);
    public override bool IsAvailable => ViewModel.IPKFilePath.FileExists;

    public override IEnumerable<string> GetAppliedUtilities()
    {
        if (ViewModel.IsApplied)
            yield return Resources.RLU_UbiRayHeader;
    }
}