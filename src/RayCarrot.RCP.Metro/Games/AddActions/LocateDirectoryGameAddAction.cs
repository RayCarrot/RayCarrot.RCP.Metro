﻿namespace RayCarrot.RCP.Metro;

public class LocateDirectoryGameAddAction : GameAddAction
{
    public LocateDirectoryGameAddAction(GameDescriptor gameDescriptor, bool singleInstallationOnly = false)
    {
        GameDescriptor = gameDescriptor;
        _singleInstallationOnly = singleInstallationOnly;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly bool _singleInstallationOnly;

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.GameDisplay_Locate));
    public override GenericIconKind Icon => GenericIconKind.GameAdd_Locate;
    public override bool IsAvailable => !_singleInstallationOnly || !Services.Games.AnyInstalledGames(x => x.GameId == GameDescriptor.GameId);

    public GameDescriptor GameDescriptor { get; }
    
    public override async Task<GameInstallation?> AddGameAsync()
    {
        Logger.Trace("Adding the game {0} through locating directory", GameDescriptor.GameId);

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

        InstallLocation location = new(result.SelectedDirectory);

        // Make sure the location is valid
        GameLocationValidationResult validationResult = GameDescriptor.ValidateLocation(location, GameValidationFlags.All);

        if (!validationResult.IsValid)
        {
            Logger.Info("The selected install directory for {0} is not valid", GameDescriptor.GameId);

            await Services.MessageUI.DisplayMessageAsync(
                $"{Resources.LocateGame_InvalidLocation}{Environment.NewLine}{Environment.NewLine}{validationResult.ErrorMessage}",
                Resources.LocateGame_InvalidLocationHeader, MessageType.Error);

            return null;
        }

        // Add the game
        return await Services.Games.AddGameAsync(GameDescriptor, location);
    }
}