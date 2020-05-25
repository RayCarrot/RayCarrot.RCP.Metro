using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Common;
using RayCarrot.Logging;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Designer replace files utility
    /// </summary>
    public class RDReplaceFilesUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RDReplaceFilesUtilityViewModel()
        {
            // Create commands
            ReplaceRayKitCommand = new AsyncRelayCommand(ReplaceRayKitAsync);

            // Default properties
            MapperLanguage = RaymanDesignerMapperLanguage.English;
        }

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
            RL.Logger?.LogInformationSource($"The Rayman Designer replacement patch is downloading...");

            // Find the files to be replaced
            var files = new Tuple<string, Uri>[]
            {
                new Tuple<string, Uri>("CLIENT.EXE", new Uri(CommonUrls.RD_ClientExe_URL)),
                new Tuple<string, Uri>("RAYRUN.EXE", new Uri(CommonUrls.RD_RayrunExe_URL)),
                new Tuple<string, Uri>("STARTUP.EXE", new Uri(CommonUrls.RD_StartupExe_URL)),
                new Tuple<string, Uri>("MAPPER.EXE", new Uri(
                    MapperLanguage == RaymanDesignerMapperLanguage.English ? CommonUrls.RD_USMapperExe_URL :
                        MapperLanguage == RaymanDesignerMapperLanguage.French ? CommonUrls.RD_FRMapperExe_URL : CommonUrls.RD_ALMapperExe_URL)),
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

            RL.Logger?.LogInformationSource($"The following Rayman Designer files were found to replace: {foundFiles.Select(x => x.Item1.Name).JoinItems(", ")}");

            await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.RDU_ReplaceFiles_InfoMessage, foundFiles.Count, files.Length), MessageType.Information);

            try
            {
                // Get the download groups
                var groups = foundFiles.GroupBy(x => x.Item1);

                // Download each group
                foreach (var group in groups)
                    // Download the files
                    await App.DownloadAsync(group.Select(x => x.Item2).ToList(), false, group.Key);

                RL.Logger?.LogInformationSource($"The Rayman Designer files have been replaced");

                await Services.MessageUI.DisplayMessageAsync(Resources.RDU_ReplaceFiles_Complete, MessageType.Information);
            }
            catch (Exception ex)
            {
                ex.HandleError("Replacing R1 soundtrack");
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
}