using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro.Games.Clients.MGba;

public sealed class VisualBoyAdvanceMGameClientDescriptor : EmulatorGameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "VisualBoyAdvanceM";
    public override bool InstallationRequiresFile => true;
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.Gba };
    public override LocalizedString DisplayName => "Visual Boy Advance - M"; // TODO-UPDATE: Localize
    public override GameClientIconAsset Icon => GameClientIconAsset.VisualBoyAdvanceM;

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent, DefaultGameClientLaunchGameComponent>();
        builder.Register<EmulatedSaveFilesComponent, VisualBoyAdvanceMEmulatedSaveFilesComponent>();
    }

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new Win32ShortcutFinderQuery("visualboyadvance-m") { FileName = "visualboyadvance-m.exe" },
    };

    #endregion
}