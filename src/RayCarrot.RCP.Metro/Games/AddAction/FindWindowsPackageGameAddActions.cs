using System;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

public class FindWindowsPackageGameAddActions : GameAddAction
{
    public FindWindowsPackageGameAddActions(WindowsPackageGameDescriptor gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override LocalizedString Header => "Find"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.GameAdd_Find;

    // Windows package games can only be added once
    public override bool IsAvailable => !Services.Games.EnumerateInstalledGames(GameDescriptor.GameId).Any();

    public WindowsPackageGameDescriptor GameDescriptor { get; }

    public override async Task<GameInstallation?> AddGameAsync()
    {
        // Make sure version is at least Windows 8
        if (!WindowsPackageGameDescriptor.SupportsWinRT)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_WinStoreNotSupported, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

            return null;
        }

        FileSystemPath installDir;

        try
        {
            // Get the install directory
            string? dir = GameDescriptor.GetPackageInstallDirectory();

            // Make sure we got a valid directory
            if (dir == null)
            {
                Logger.Info("The {0} was not found under Windows Store packages", GameDescriptor.GameId);

                return null;
            }

            installDir = dir;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting Windows Store game install directory");

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

            return null;
        }

        if (!GameDescriptor.IsValid(installDir))
        {
            Logger.Info("The {0} install directory was not valid", GameDescriptor.GameId);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

            return null;
        }

        return await Services.Games.AddGameAsync(GameDescriptor, installDir);
    }
}