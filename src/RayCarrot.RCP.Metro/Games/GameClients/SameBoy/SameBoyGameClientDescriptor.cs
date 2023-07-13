using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro.Games.Clients.MGba;

public sealed class SameBoyGameClientDescriptor : EmulatorGameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "SameBoy";
    public override bool InstallationRequiresFile => true;
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.Gbc };
    public override LocalizedString DisplayName => "SameBoy"; // TODO-UPDATE: Localize
    public override GameClientIconAsset Icon => GameClientIconAsset.SameBoy;

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent, DefaultGameClientLaunchGameComponent>();
        // For now we can avoid registering a save file component since no GBC games support progression in RCP
        //builder.Register<EmulatedSaveFilesComponent, >();
    }

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new Win32ShortcutFinderQuery("SameBoy") { FileName = "sameboy.exe" },
    };

    #endregion
}