namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins update utility
/// </summary>
public class Utility_RaymanOrigins_Update : Utility<Utility_RaymanOrigins_Update_Control, Utility_RaymanOrigins_Update_ViewModel>
{
    public Utility_RaymanOrigins_Update(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.ROU_UpdateHeader));
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanOrigins_Update;
    public override LocalizedString InfoText => new ResourceLocString(nameof(Resources.ROU_UpdateInfo));
    public override bool RequiresAdditionalFiles => true;
}