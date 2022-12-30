using Microsoft.Win32;

namespace RayCarrot.RCP.Metro;

// TODO-14: If this action finds the game we should default to use the Steam client if there is one
public class FindSteamGameAddAction : GameAddAction
{
    public FindSteamGameAddAction(GameDescriptor gameDescriptor, string steamId)
    {
        GameDescriptor = gameDescriptor;
        SteamId = steamId;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override LocalizedString Header => "Find Steam"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.GameAdd_Find;

    public override bool IsAvailable => true;

    public GameDescriptor GameDescriptor { get; }
    public string SteamId { get; }

    public override async Task<GameInstallation?> AddGameAsync()
    {
        FileSystemPath installDir;

        try
        {
            // TODO-UPDATE: Fix messages

            // Get the key path
            var keyPath = RegistryHelpers.CombinePaths(AppFilePaths.UninstallRegistryKey, $"Steam App {SteamId}");

            using var key = RegistryHelpers.GetKeyFromFullPath(keyPath, RegistryView.Registry64);

            // Get the install directory
            if (key?.GetValue("InstallLocation") is not string dir)
            {
                Logger.Info("The {0} was not found under Steam Apps", GameDescriptor.GameId);

                await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

                return null;
            }

            installDir = dir;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting Steam game install directory");

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

            return null;
        }

        // Make sure the game is valid
        if (!GameDescriptor.IsValid(installDir))
        {
            Logger.Info("The {0} install directory was not valid", GameDescriptor.GameId);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

            return null;
        }

        return await Services.Games.AddGameAsync(GameDescriptor, installDir);
    }
}