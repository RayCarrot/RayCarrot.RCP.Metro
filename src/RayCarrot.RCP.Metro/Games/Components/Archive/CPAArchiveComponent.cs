using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(BinaryGameModeComponent))]
public class CPAArchiveComponent : ArchiveComponent
{
    public CPAArchiveComponent(Func<GameInstallation, IEnumerable<string>> archivePathsFunc) 
        : base(GetArchiveManager, archivePathsFunc, Id) { }

    private new const string Id = "CPA_CNT";

    private static (OpenSpaceSettings Settings, CPAGameMode GameMode) GetSettings(GameInstallation gameInstallation)
    {
        BinaryGameModeComponent gameModeComponent = gameInstallation.GetRequiredComponent<BinaryGameModeComponent>();

        if (gameModeComponent.GameModeAttribute.GetSettingsObject() is not OpenSpaceSettings openSpaceSettings)
            throw new Exception($"The settings object provided by the corresponding game mode {gameModeComponent.GameMode} is not of the correct type");

        return (openSpaceSettings, (CPAGameMode)gameModeComponent.GameMode);
    }

    private static IArchiveDataManager GetArchiveManager(GameInstallation gameInstallation)
    {
        (OpenSpaceSettings? settings, CPAGameMode gameMode) = GetSettings(gameInstallation);

        CPATextureSyncData? textureSyncData = null;

        if (CPATextureSyncData.SupportedGameModes.Contains(gameMode))
            textureSyncData = CPATextureSyncData.FromGameMode(gameMode);

        return new CPACntArchiveDataManager(
            settings: settings,
            gameInstallation: gameInstallation,
            cpaTextureSyncData: textureSyncData);
    }

    private static async Task SynchronizeTexturesAsync(GameInstallation gameInstallation)
    {
        CPATextureSyncData textureSyncData = CPATextureSyncData.FromGameMode(GetSettings(gameInstallation).GameMode);

        CPATextureSyncManager textureSyncManager = new(gameInstallation, textureSyncData);

        // TODO-UPDATE: Localize
        using (LoadState state = await Services.App.LoaderViewModel.RunAsync("Synchronizing textures"))
        {
            await textureSyncManager.SyncTextureInfoAsync(progressCallback: state.SetProgress);
        }
    }

    public override AdditionalArchiveAction? GetAdditionalAction()
    {
        if (!CPATextureSyncData.SupportedGameModes.Contains(GetSettings(GameInstallation).GameMode))
            return null;

        return new AdditionalArchiveAction(
            GenericIconKind.ArchiveAdditionalAction_CPATextureSync,
            // TODO-UPDATE: Localize
            "Synchronize textures. This is required to do if the resolution of a texture has been increased.",
            SynchronizeTexturesAsync);
    }
}