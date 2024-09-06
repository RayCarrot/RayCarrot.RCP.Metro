namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameManager_RaymanForever_MsDos : SetupGameManager
{
    public SetupGameManager_RaymanForever_MsDos(GameInstallation gameInstallation) : base(gameInstallation) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static FileSystemPath GetMusicDirectory(GameInstallation gameInstallation)
    {
        return gameInstallation.InstallLocation.Directory.Parent + "Music";
    }

    private bool? IsOriginalSoundtrack(FileSystemPath musicDir)
    {
        try
        {
            FileSystemPath file = musicDir + "rayman02.ogg";

            if (!file.FileExists)
                return null;

            long size = file.GetSize();

            return size == 1805221;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting Rayman Forever music size");
            return null;
        }
    }

    private async Task ReplaceSoundtrackAsync(FileSystemPath musicDir)
    {
        try
        {
            Logger.Info("The Rayman Forever soundtrack is being replaced");

            // Download the files
            bool success = await Services.App.DownloadAsync(new[]
            {
                new Uri(AppURLs.R1_CompleteOST_URL)
            }, true, musicDir);

            if (success)
            {
                // Find all Rayman Forever games which use this music directory and refresh them
                List<GameInstallation> games = new();
                foreach (GameInstallation gameInstallation in Services.Games.GetInstalledGames())
                {
                    if (gameInstallation.GameDescriptor.Game is Game.Rayman1 or Game.RaymanDesigner or Game.RaymanByHisFans &&
                        gameInstallation.GameDescriptor.Platform == GamePlatform.MsDos)
                    {
                        if (GetMusicDirectory(gameInstallation) == musicDir)
                            games.Add(gameInstallation);
                    }
                }

                Services.Messenger.Send(new FixedSetupGameActionMessage(games));
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Replacing Rayman Forever soundtrack");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_CompleteOSTReplaceError);
        }
    }

    public override IEnumerable<SetupGameAction> GetRecommendedActions()
    {
        // Get the music directory for Rayman Forever
        FileSystemPath musicDir = GetMusicDirectory(GameInstallation);

        // Make sure it exists
        if (musicDir.DirectoryExists)
        {
            // Check if it's the original soundtrack
            bool? isOriginalSoundtrack = IsOriginalSoundtrack(musicDir);
            if (isOriginalSoundtrack != null)
            {
                // TODO-LOC
                // Replace incomplete soundtrack
                yield return new SetupGameAction(
                    header: "Replace incomplete soundtrack",
                    info: "The Rayman Forever collection does not come with the full soundtrack for the game due to limited disc space in the original release. It is recommended to replace the music files with those from the complete soundtrack.",
                    isComplete: isOriginalSoundtrack == false,
                    fixActionIcon: GenericIconKind.SetupGame_FileReplacement,
                    fixActionDisplayName: "Replace with complete soundtrack",
                    fixAction: async () => await ReplaceSoundtrackAsync(musicDir));
            }
        }
    }
}