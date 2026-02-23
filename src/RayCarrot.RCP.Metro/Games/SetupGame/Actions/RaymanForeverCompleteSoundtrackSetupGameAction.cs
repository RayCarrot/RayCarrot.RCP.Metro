namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class RaymanForeverCompleteSoundtrackSetupGameAction : SetupGameAction
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_RaymanForeverCompleteSoundtrack_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_RaymanForeverCompleteSoundtrack_Info));

    public override SetupGameActionType Type => SetupGameActionType.Recommended;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_FileReplacement;
    public override LocalizedString FixActionDisplayName => new ResourceLocString(nameof(Resources.SetupGameAction_RaymanForeverCompleteSoundtrack_Fix));

    private static FileSystemPath GetMusicDirectory(GameInstallation gameInstallation)
    {
        return gameInstallation.InstallLocation.Directory.Parent + "Music";
    }

    private static bool? IsOriginalSoundtrack(FileSystemPath musicDir)
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

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        // Get the music directory for Rayman Forever
        FileSystemPath musicDir = GetMusicDirectory(gameInstallation);

        // Make sure it exists
        if (!musicDir.DirectoryExists) 
            return false;
        
        // Make sure the soundtrack exists
        return IsOriginalSoundtrack(musicDir) != null;
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        FileSystemPath musicDir = GetMusicDirectory(gameInstallation);
        return IsOriginalSoundtrack(musicDir) == false;
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        try
        {
            // Get the music directory for Rayman Forever
            FileSystemPath musicDir = GetMusicDirectory(gameInstallation);

            Logger.Info("The Rayman Forever soundtrack is being replaced");

            // Download the files
            bool success = await Services.App.DownloadGameBananaFileAsync(AppURLs.RaymanForeverCompleteSoundtrackGameBananaFileId, musicDir);

            if (success)
            {
                // Find all Rayman Forever games which use this music directory and refresh them
                List<GameInstallation> games = new();
                foreach (GameInstallation g in Services.Games.GetInstalledGames())
                {
                    if (g.GameDescriptor.Game is Game.Rayman1 or Game.RaymanDesigner or Game.RaymanByHisFans &&
                        g.GameDescriptor.Platform == GamePlatform.MsDos)
                    {
                        if (GetMusicDirectory(g) == musicDir)
                            games.Add(g);
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
}