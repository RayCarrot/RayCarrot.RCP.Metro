using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(BinaryGameModeComponent))]
public class CPAArchiveComponent : ArchiveComponent
{
    public CPAArchiveComponent(Func<GameInstallation, IEnumerable<string>> archivePathsFunc) 
        : base(GetArchiveManager, archivePathsFunc, Id) { }

    public new const string Id = "CPA_CNT";

    private static IArchiveDataManager GetArchiveManager(GameInstallation gameInstallation)
    {
        OpenSpaceSettings settings = gameInstallation.
            GetRequiredComponent<BinaryGameModeComponent, CPAGameModeComponent>().
            GetSettings();

        return new CPACntArchiveDataManager(
            settings: settings,
            gameInstallation: gameInstallation,
            cpaTextureSyncItems: gameInstallation.GetComponent<CPATextureSyncComponent>()?.TextureSyncItems);
    }

    private static async Task SynchronizeTexturesAsync(GameInstallation gameInstallation)
    {
        CPATextureSyncDataItem[] textureSyncItems = gameInstallation.GetRequiredComponent<CPATextureSyncComponent>().TextureSyncItems;

        OpenSpaceSettings settings = gameInstallation.
            GetRequiredComponent<BinaryGameModeComponent, CPAGameModeComponent>().
            GetSettings();

        CPATextureSyncManager textureSyncManager = new(gameInstallation, settings, textureSyncItems);

        using (LoadState state = await Services.App.LoaderViewModel.RunAsync(Resources.Utilities_SyncTextureInfo_SyncStatus))
        {
            await textureSyncManager.SyncTextureInfoAsync(progressCallback: state.SetProgress);
        }
    }

    public override AdditionalArchiveAction? GetAdditionalAction()
    {
        if (!GameInstallation.HasComponent<CPATextureSyncComponent>())
            return null;

        return new AdditionalArchiveAction(
            new ResourceLocString(nameof(Resources.Utilities_SyncTextureInfo)),
            new ResourceLocString(nameof(Resources.Utilities_SyncTextureInfo_Info)),
            SynchronizeTexturesAsync);
    }
}