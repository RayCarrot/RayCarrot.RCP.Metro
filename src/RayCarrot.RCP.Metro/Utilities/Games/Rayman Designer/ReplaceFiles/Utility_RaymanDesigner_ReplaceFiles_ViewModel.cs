#nullable disable
using RayCarrot.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Designer replace files utility
/// </summary>
public class Utility_RaymanDesigner_ReplaceFiles_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanDesigner_ReplaceFiles_ViewModel()
    {
        // Create commands
        ReplaceRayKitCommand = new AsyncRelayCommand(ReplaceRayKitAsync);

        // Default properties
        MapperLanguage = RaymanDesignerMapperLanguage.English;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ReplaceRayKitCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Replaces the infected Rayman Designer files
    /// </summary>
    /// <returns>The task</returns>
    public async Task ReplaceRayKitAsync()
    {
        Logger.Info("The Rayman Designer replacement patch is downloading...");

        // Find the files to be replaced
        var files = new Tuple<string, Uri>[]
        {
            new Tuple<string, Uri>("CLIENT.EXE", new Uri(AppURLs.RD_ClientExe_URL)),
            new Tuple<string, Uri>("RAYRUN.EXE", new Uri(AppURLs.RD_RayrunExe_URL)),
            new Tuple<string, Uri>("STARTUP.EXE", new Uri(AppURLs.RD_StartupExe_URL)),
            new Tuple<string, Uri>("MAPPER.EXE", new Uri(
                MapperLanguage == RaymanDesignerMapperLanguage.English ? AppURLs.RD_USMapperExe_URL :
                MapperLanguage == RaymanDesignerMapperLanguage.French ? AppURLs.RD_FRMapperExe_URL : AppURLs.RD_ALMapperExe_URL)),
        };

        // Get the game install dir
        var installDir = Games.RaymanDesigner.GetInstallDir();

        // Find the directories to search
        var dirs = new FileSystemPath[]
        {
            installDir,
            installDir + "OSD"
        };

        // Keep track of the found files
        var foundFiles = new List<Tuple<FileSystemPath, Uri>>();

        // Search for the files
        foreach ((var fileName, Uri fileUrl) in files)
        {
            // Check each directory
            foreach (var dir in dirs)
            {
                // Get the path
                var path = dir + fileName;

                // Check if the path exists
                if (!path.FileExists)
                    continue;

                foundFiles.Add(new Tuple<FileSystemPath, Uri>(dir, fileUrl));
                break;
            }
        }

        Logger.Info("The following Rayman Designer files were found to replace: {0}", foundFiles.Select(x => x.Item1.Name).JoinItems(", "));

        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.RDU_ReplaceFiles_InfoMessage, foundFiles.Count, files.Length), MessageType.Information);

        try
        {
            // Get the download groups
            var groups = foundFiles.GroupBy(x => x.Item1);

            // Download each group
            foreach (var group in groups)
                // Download the files
                await App.DownloadAsync(group.Select(x => x.Item2).ToArray(), false, group.Key);

            Logger.Info("The Rayman Designer files have been replaced");

            await Services.MessageUI.DisplayMessageAsync(Resources.RDU_ReplaceFiles_Complete, MessageType.Information);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Replacing R1 soundtrack");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.RDU_ReplaceFiles_Error);
        }
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The selected Mapper language
    /// </summary>
    public RaymanDesignerMapperLanguage MapperLanguage { get; set; }

    #endregion

    #region Public Enums

    /// <summary>
    /// The available Rayman Designer Mapper languages
    /// </summary>
    public enum RaymanDesignerMapperLanguage
    {
        English,
        German,
        French
    }

    #endregion
}