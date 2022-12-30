namespace RayCarrot.RCP.Metro.Games.Components;

public class RaymanMArenaSetupLocalGameLinksComponent : LocalGameLinksComponent
{
    public RaymanMArenaSetupLocalGameLinksComponent() : base(GetLocalGameLinks) { }

    private static IEnumerable<GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_Setup)),
            Uri: gameInstallation.InstallLocation + "RM_Setup_DX8.exe")
    };
}