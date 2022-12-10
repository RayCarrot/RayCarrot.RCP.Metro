using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using NLog;

namespace RayCarrot.RCP.Metro;

public class FindSteamGameAddAction : GameAddAction
{
    public FindSteamGameAddAction(SteamGameDescriptor gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override LocalizedString Header => "Find"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.GameAdd_Find;

    // Steam games can only be added once
    public override bool IsAvailable => !Services.Games.AnyInstalledGames(x => x.GameId == GameDescriptor.GameId);

    public SteamGameDescriptor GameDescriptor { get; }

    public override async Task<GameInstallation?> AddGameAsync()
    {
        FileSystemPath installDir;

        try
        {
            // TODO-UPDATE: Fix messages

            // Get the key path
            var keyPath = RegistryHelpers.CombinePaths(AppFilePaths.UninstallRegistryKey, $"Steam App {GameDescriptor.SteamID}");

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