using System;
using System.IO;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

public class LocateRayman1MSDOSGameAddAction : GameAddAction
{
    public LocateRayman1MSDOSGameAddAction(MSDOSGameDescriptor gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.GameDisplay_Locate));
    public override GenericIconKind Icon => GenericIconKind.GameAdd_Locate;
    public override bool IsAvailable => true;

    public MSDOSGameDescriptor GameDescriptor { get; }

    public override async Task<GameInstallation?> AddGameAsync()
    {
        // Have user browse for directory
        var result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
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
        if (GameDescriptor.IsValid(result.SelectedDirectory))
            return await Services.Games.AddGameAsync(GameDescriptor, result.SelectedDirectory);
        
        // If the executable does not exist the location is not valid
        if (!(result.SelectedDirectory + GameDescriptor.ExecutableName).FileExists)
        {
            Logger.Info("The selected install directory for {0} is not valid", GameDescriptor.Id);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
            return null;
        }

        // Create the .bat file
        File.WriteAllLines(result.SelectedDirectory + GameDescriptor.DefaultFileName, new[]
        {
            "@echo off",
            $"{Path.GetFileNameWithoutExtension(GameDescriptor.ExecutableName)} ver=usa"
        });

        Logger.Info("A batch file was created for {0}", GameDescriptor.Id);

        return await Services.Games.AddGameAsync(GameDescriptor, result.SelectedDirectory);
    }
}