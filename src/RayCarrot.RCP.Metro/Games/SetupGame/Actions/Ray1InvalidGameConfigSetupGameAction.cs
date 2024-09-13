using BinarySerializer;
using BinarySerializer.Ray1.PC;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class Ray1InvalidGameConfigSetupGameAction : SetupGameAction
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // TODO-LOC
    public override LocalizedString Header => "Game is not correctly configured";
    public override LocalizedString Info => "Certain settings in the game config are not set correctly. This could cause issues in the game such as missing sound effects. Go to the game config, choose the recommended settings and save to fix it.";

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Config;
    public override LocalizedString? FixActionDisplayName => "Open config"; // TODO-LOC

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        // Get the config file name
        string configFileName = gameInstallation.GetRequiredComponent<Ray1ConfigFileNameComponent>().CreateObject();

        // Create the context to use
        using Context context = new RCPContext(gameInstallation.InstallLocation.Directory);
        context.Initialize(gameInstallation);
        ConfigFile? config;

        try
        {

            config = context.ReadFileData<ConfigFile>(configFileName);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading Ray1 config");
            return false;
        }

        // Config has not been created, in which case we do want to show this action
        if (config == null)
            return true;

        // Check values
        return config.Port != 544 || 
               config.Irq != 5 || 
               config.Dma != 5 || 
               config.DeviceID != 57368 || 
               config.NumCard != 3;
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        return false;
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        await Services.UI.ShowGameOptionsAsync(gameInstallation);
        Services.Messenger.Send(new FixedSetupGameActionMessage(gameInstallation));
    }
}