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

    private static OpenSpaceSettings GetSettings(GameInstallation gameInstallation)
    {
        BinaryGameModeComponent gameModeComponent = gameInstallation.GetRequiredComponent<BinaryGameModeComponent>();

        if (gameModeComponent.GameModeAttribute.GetSettingsObject() is not OpenSpaceSettings openSpaceSettings)
            throw new Exception($"The settings object provided by the corresponding game mode {gameModeComponent.GameMode} is not of the correct type");

        return openSpaceSettings;
    }

    private static IArchiveDataManager GetArchiveManager(GameInstallation gameInstallation)
    {
        OpenSpaceSettings settings = GetSettings(gameInstallation);

        return new CPACntArchiveDataManager(
            settings: settings,
            gameInstallation: gameInstallation,
            cpaTextureSyncItems: gameInstallation.GetComponent<CPATextureSyncComponent>()?.TextureSyncItems);
    }

    private static async Task SynchronizeTexturesAsync(GameInstallation gameInstallation)
    {
        CPATextureSyncDataItem[] textureSyncItems = gameInstallation.GetRequiredComponent<CPATextureSyncComponent>().TextureSyncItems;

        CPATextureSyncManager textureSyncManager = new(gameInstallation, GetSettings(gameInstallation), textureSyncItems);

        // TODO-UPDATE: Localize
        using (LoadState state = await Services.App.LoaderViewModel.RunAsync("Synchronizing textures"))
        {
            await textureSyncManager.SyncTextureInfoAsync(progressCallback: state.SetProgress);
        }
    }

    public override AdditionalArchiveAction? GetAdditionalAction()
    {
        if (!GameInstallation.HasComponent<CPATextureSyncComponent>())
            return null;

        return new AdditionalArchiveAction(
            GenericIconKind.ArchiveAdditionalAction_CPATextureSync,
            // TODO-UPDATE: Localize
            "Synchronize textures. This updates the texture sizes in the game files to match the textures themselves. This is required to do if the resolution of a texture has been increased or else the game will crash.",
            SynchronizeTexturesAsync);
    }
}