using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro;

public class DownloadGameAddAction : GameAddAction
{
    public DownloadGameAddAction(GameDescriptor gameDescriptor, Uri[] downloadUrls)
    {
        GameDescriptor = gameDescriptor;
        DownloadUrls = downloadUrls;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override LocalizedString Header => "Download"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.GameAdd_Download;

    // Can only be downloaded once
    public override bool IsAvailable => Services.Games.GetInstalledGames().
        Where(x => x.GameId == GameDescriptor.GameId).
        All(x => x.GetObject<RCPGameInstallData>(GameDataKey.RCP_GameInstallData)?.InstallMode 
                 != RCPGameInstallData.RCPInstallMode.Download);

    public GameDescriptor GameDescriptor { get; }
    public Uri[] DownloadUrls { get; }

    public override async Task<GameInstallation?> AddGameAsync()
    {
        Logger.Trace("The game {0} is being downloaded...", GameDescriptor.GameId);

        // Get the game directory. Since it can only be downloaded once we use the game id.
        FileSystemPath gameDir = AppFilePaths.GamesBaseDir + GameDescriptor.GameId;

        // Download the game
        bool downloaded = await Services.App.DownloadAsync(DownloadUrls, true, gameDir, true);

        if (!downloaded)
            return null;

        // Add the game
        GameInstallation gameInstallation = await Services.Games.AddGameAsync(GameDescriptor, gameDir, x =>
        {
            // Set the install info
            RCPGameInstallData installData = new(gameDir, RCPGameInstallData.RCPInstallMode.Download);
            x.SetObject(GameDataKey.RCP_GameInstallData, installData);
        });

        Logger.Trace("The game {0} has been downloaded", GameDescriptor.GameId);

        // TODO-UPDATE: Should we keep this?
        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.GameInstall_Success, GameDescriptor.DisplayName), Resources.GameInstall_SuccessHeader);

        return gameInstallation;
    }
}