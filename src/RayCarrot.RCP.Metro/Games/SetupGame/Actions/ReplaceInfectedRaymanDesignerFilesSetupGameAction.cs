namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class ReplaceInfectedRaymanDesignerFilesSetupGameAction : SetupGameAction
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // SHA-256
    private const string InfectedMapperHash = "BE0DB1033728D8FD87256FDD232DF331E65F04A352C6290EB488F9FB603BA28E";
    private const string CleanMapperHash = "8BAFDC4624F272817F26FF3A1AE9527A21069A3B1711F510D03751A4F12ED5A9";

    // TODO-LOC
    public override LocalizedString Header => "Replace infected files";
    public override LocalizedString Info => "Some versions of Rayman Designer, such as the one bundled in Rayman Forever, has some of its files infected with an old malware. Although it does not appear to be harmful, or even active, and GOG has confirmed there is no danger, it can still be helpful to replace the files with their non-infected versions to avoid anti-virus programs from removing the game files.";

    public override SetupGameActionType Type => SetupGameActionType.Optional;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_FileReplacement;
    public override LocalizedString? FixActionDisplayName => "Replace infected files"; // TODO-LOC

    private FileSystemPath GetMapperFilePath(GameInstallation gameInstallation)
    {
        return gameInstallation.InstallLocation.Directory + "MAPPER.EXE";
    }

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        FileSystemPath mapperFilePath = GetMapperFilePath(gameInstallation);

        if (!mapperFilePath.FileExists)
            return false;

        try
        {
            string hash = mapperFilePath.GetSHA256CheckSum();
            return hash is InfectedMapperHash or CleanMapperHash;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting Mapper hash");
            return false;
        }
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        try
        {
            string hash = (gameInstallation.InstallLocation.Directory + "MAPPER.EXE").GetSHA256CheckSum();
            return hash != InfectedMapperHash;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting Mapper hash");
            return false;
        }
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        try
        {
            // Download the files
            await Services.App.DownloadAsync(new Uri[]
            {
                new(AppURLs.RD_CleanFiles_URL)
            }, isCompressed: true, outputDir: gameInstallation.InstallLocation.Directory);

            Logger.Info("The Rayman Designer files have been replaced");

            await Services.MessageUI.DisplayMessageAsync(Resources.RDU_ReplaceFiles_Complete, MessageType.Information);

            Services.Messenger.Send(new FixedSetupGameActionMessage(gameInstallation));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Replacing infected Rayman Designer soundtrack");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.RDU_ReplaceFiles_Error);
        }
    }
}