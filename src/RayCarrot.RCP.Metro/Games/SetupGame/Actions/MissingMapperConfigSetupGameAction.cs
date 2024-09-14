using System.IO;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class MissingMapperConfigSetupGameAction : SetupGameAction
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static string DefaultConfigText => """
                                               [OSD]
                                               Directory =.\ubisoft\osd
                                               Valid = TRUE

                                               [INSTALLED PRODUCTS]
                                               RAYKIT

                                               [RAYKIT]
                                               SrcDataPath =\
                                               Directory =.\
                                               """;

    // TODO-LOC
    public override LocalizedString Header => "Mapper config file is missing or invalid";
    public override LocalizedString Info => "During some Rayman Designer installations the configuration file won't be created, or created with invalid values, causing the Mapper editor to give an error when launching.";

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Fix;
    public override LocalizedString? FixActionDisplayName => "Fix"; // TODO-LOC

    private FileSystemPath GetConfigFilePath(GameInstallation gameInstallation)
    {
        return gameInstallation.InstallLocation.Directory + @"Ubisoft\ubi.ini";
    }

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        // Get the config path
        FileSystemPath configFilePath = GetConfigFilePath(gameInstallation);

        // Show action if the config file doesn't exist
        if (!configFilePath.FileExists)
            return true;

        // Verify the directory value
        string dirValue = IniNative.GetString(configFilePath, "RAYKIT", "Directory", String.Empty);
        return dirValue != @".\";
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        return false;
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        // Get the config path
        FileSystemPath configFilePath = GetConfigFilePath(gameInstallation);

        try
        {
            // Create the file
            Directory.CreateDirectory(configFilePath.Parent);
            File.WriteAllText(configFilePath, DefaultConfigText);

            Logger.Info("The Mapper config file has been recreated");

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.RDU_CreateConfig_Success);
            Services.Messenger.Send(new FixedSetupGameActionMessage(gameInstallation));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Creating Mapper config file");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.RDU_CreateConfig_Error);
        }
    }
}