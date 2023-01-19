namespace RayCarrot.RCP.Metro.Games.Components;

public class Rayman3SetupLocalGameLinksComponent : LocalGameLinksComponent
{
    public Rayman3SetupLocalGameLinksComponent(bool isDemo) : base(x => GetLocalGameLinks(x, isDemo)) { }

    private static IEnumerable<GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation, bool isDemo)
    {
        string setupFileName = isDemo ? "R3_Setup_DX8D.exe" : "R3_Setup_DX8.exe";

        return new[]
        {
            new GameUriLink(
                Header: new ResourceLocString(nameof(Resources.GameLink_Setup)),
                Uri: gameInstallation.InstallLocation.Directory + setupFileName)
        };
    }
}