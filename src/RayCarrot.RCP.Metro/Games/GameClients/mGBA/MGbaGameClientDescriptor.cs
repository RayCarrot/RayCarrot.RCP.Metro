using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

public sealed class MGbaGameClientDescriptor : EmulatorGameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "mGBA";
    public override bool InstallationRequiresFile => true;
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.Gba };
    public override LocalizedString DisplayName => "mGBA"; // TODO-UPDATE: Localize
    public override GameClientIconAsset Icon => GameClientIconAsset.MGBA;

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent, DefaultGameClientLaunchGameComponent>();
    }

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        // Don't include this for now since the name has the version number in it
        // and currently we check for an exact string match
        //new UninstallProgramFinderQuery("mGBA"),

        new Win32ShortcutFinderQuery("mGBA") { FileName = "mGBA.exe" },
    };

    #endregion
}