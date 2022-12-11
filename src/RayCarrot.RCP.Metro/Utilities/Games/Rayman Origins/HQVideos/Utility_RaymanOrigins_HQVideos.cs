namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins HQ videos utility
/// </summary>
public class Utility_RaymanOrigins_HQVideos : Utility<Utility_RaymanOrigins_HQVideos_Control, Utility_RaymanOrigins_HQVideos_ViewModel>
{
    public Utility_RaymanOrigins_HQVideos(GameInstallation gameInstallation) 
        : base(new Utility_RaymanOrigins_HQVideos_ViewModel(gameInstallation))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public override string DisplayHeader => Resources.ROU_HQVideosHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanOrigins_HQVideos;
    public override string InfoText => Resources.ROU_HQVideosInfo;
    public override bool RequiresAdditionalFiles => true; 
    public override bool RequiresAdmin => !Services.File.CheckFileWriteAccess(ViewModel.VideoDir + "intro.bik");
    public override bool IsAvailable => ViewModel.CanVideosBeReplaced;
    public override IEnumerable<string> GetAppliedUtilities()
    {
        if (ViewModel.GetIsOriginalVideos() == false)
            yield return Resources.ROU_HQVideosHeader;
    }
}