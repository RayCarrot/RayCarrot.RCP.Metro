namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 TPLS utility
/// </summary>
public class Utility_Rayman1_TPLS : Utility<Utility_Rayman1_TPLS_Control, Utility_Rayman1_TPLS_ViewModel>
{
    public Utility_Rayman1_TPLS(GameInstallation gameInstallation)
        : base(new Utility_Rayman1_TPLS_ViewModel(gameInstallation))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.R1U_TPLSHeader));
    public override GenericIconKind Icon => GenericIconKind.Utilities_Rayman1_TPLS;
    public override LocalizedString InfoText => new ResourceLocString(nameof(Resources.R1U_TPLSInfo));
    public override bool RequiresAdditionalFiles => true;
}