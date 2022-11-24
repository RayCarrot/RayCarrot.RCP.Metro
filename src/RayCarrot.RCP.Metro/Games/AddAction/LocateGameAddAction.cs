using System;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

public class LocateGameAddAction : GameAddAction
{
    public LocateGameAddAction(GameDescriptor gameDescriptor, bool singleInstallationOnly = false)
    {
        GameDescriptor = gameDescriptor;
        _singleInstallationOnly = singleInstallationOnly;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly bool _singleInstallationOnly;

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.GameDisplay_Locate));
    public override GenericIconKind Icon => GenericIconKind.GameAdd_Locate;
    public override bool IsAvailable => !_singleInstallationOnly || !Services.Games.EnumerateInstalledGames(GameDescriptor.Id).Any();

    public GameDescriptor GameDescriptor { get; }
    
    public override async Task<GameInstallation?> AddGameAsync()
    {
        // Have user browse for directory
        DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
        {
            Title = Resources.LocateGame_BrowserHeader,
            DefaultDirectory = Environment.SpecialFolder.ProgramFilesX86.GetFolderPath(),
            MultiSelection = false
        });

        // Make sure the user did not cancel
        if (result.CanceledByUser)
            return null;

        // Make sure the selected directory exists
        if (!result.SelectedDirectory.DirectoryExists)
            return null;

        // Make sure the directory is valid
        if (!await GameDescriptor.IsValidAsync(result.SelectedDirectory))
        {
            Logger.Info("The selected install directory for {0} is not valid", GameDescriptor.Id);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation,
                Resources.LocateGame_InvalidLocationHeader, MessageType.Error);

            return null;
        }

        // Add the game
        return await Services.Games.AddGameAsync(GameDescriptor, result.SelectedDirectory);
    }
}