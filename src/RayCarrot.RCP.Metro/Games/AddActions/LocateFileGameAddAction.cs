using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

public class LocateFileGameAddAction : GameAddAction
{
    public LocateFileGameAddAction(GameDescriptor gameDescriptor, bool singleInstallationOnly = false)
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
        Logger.Trace("Adding the game {0} through locating file", GameDescriptor.GameId);

        FileExtension[] fileExtensions = GameDescriptor.GetStructure<SingleFileProgramInstallationStructure>().SupportedFileExtensions;

        // Have user browse for file
        FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            // TODO-UPDATE: Localize
            Title = "Select the game file",
            DefaultDirectory = Environment.SpecialFolder.ProgramFilesX86.GetFolderPath(),
            ExtensionFilter = new FileFilterItemCollection(fileExtensions).
                // TODO-UPDATE: Localize
                CombineAll("Game file").ToString(),
            MultiSelection = false
        });

        // Make sure the user did not cancel
        if (result.CanceledByUser)
            return null;

        // Make sure the selected file exists
        if (!result.SelectedFile.FileExists)
            return null;

        InstallLocation location = InstallLocation.FromFilePath(result.SelectedFile);

        // Make sure the location is valid
        GameLocationValidationResult validationResult = GameDescriptor.ValidateLocation(location);

        if (!validationResult.IsValid)
        {
            Logger.Info("The selected file for {0} is not valid", GameDescriptor.GameId);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync(
                $"The selected file is not valid for this game{Environment.NewLine}{Environment.NewLine}{validationResult.ErrorMessage}",
                Resources.LocateGame_InvalidLocationHeader, MessageType.Error);

            return null;
        }

        // Add the game
        return await Services.Games.AddGameAsync(GameDescriptor, location);
    }
}