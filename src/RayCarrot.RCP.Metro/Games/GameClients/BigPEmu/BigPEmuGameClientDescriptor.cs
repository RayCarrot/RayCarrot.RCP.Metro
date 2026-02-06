using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro.Games.Clients.MGba;

public sealed class BigPEmuGameClientDescriptor : EmulatorGameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "BigPEmu";
    public override bool InstallationRequiresFile => true;
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.Jaguar };
    public override LocalizedString DisplayName => "BigPEmu"; // TODO-LOC
    public override GameClientIconAsset Icon => GameClientIconAsset.BigPEmu;

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent, DefaultGameClientLaunchGameComponent>();
        builder.Register<EmulatedSaveFilesComponent, BigPEmuEmulatedSaveFilesComponent>();
    }

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new Win32ShortcutFinderQuery("BigPEmu") { FileName = "BigPEmu.exe" },
    };

    #endregion
}