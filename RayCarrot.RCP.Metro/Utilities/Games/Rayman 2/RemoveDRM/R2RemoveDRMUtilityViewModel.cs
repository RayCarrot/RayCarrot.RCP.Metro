using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.IO;
using RayCarrot.UI;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 2 DRM removal utility
    /// </summary>
    public class R2RemoveDRMUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R2RemoveDRMUtilityViewModel()
        {
            // Create commands
            ApplyPatchCommand = new AsyncRelayCommand(ApplyPatchAsync);
            RevertPatchCommand = new AsyncRelayCommand(RevertPatchAsync);

            // Get the offsets
            var sna = JsonConvert.DeserializeObject<Dictionary<string, uint[]>>(Files.R2_Sna_Drm_Offsets);

            // Get the base directory
            BaseDirectory = Games.Rayman2.GetInstallDir() + @"Data\World\Levels";

            try
            {
                // Get the files which should be edited
                SnaOffsets = Directory.GetFiles(BaseDirectory, "*.sna", SearchOption.AllDirectories).Select(x => new FileSystemPath(x)).Where(x => sna.ContainsKey(x.Name)).ToDictionary(x => x, y => sna[y.Name]);
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting R2 .sna files");
                SnaOffsets = new Dictionary<FileSystemPath, uint[]>();
            }

            // Check if the utility has been applied, i.e. if a backup exists
            HasBeenApplied = CommonPaths.R2RemoveDRMDir.DirectoryExists;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The base directory for the .sna files
        /// </summary>
        public FileSystemPath BaseDirectory { get; }

        /// <summary>
        /// The offsets to blank in each .sna file
        /// </summary>
        public Dictionary<FileSystemPath, uint[]> SnaOffsets { get; }

        /// <summary>
        /// Indicates if the patch has been applied
        /// </summary>
        public bool HasBeenApplied { get; set; }

        /// <summary>
        /// Indicates if the utility is loading
        /// </summary>
        public bool IsLoading { get; set; }

        #endregion

        #region Commands

        public ICommand ApplyPatchCommand { get; }

        public ICommand RevertPatchCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies the patch to the current installation
        /// </summary>
        /// <returns>The task</returns>
        public async Task ApplyPatchAsync()
        {
            try
            {
                IsLoading = true;

                await Task.Run(() =>
                {
                    // Edit every .sna file
                    foreach (var sna in SnaOffsets)
                    {
                        // Backup the file
                        RCFRCP.File.CopyFile(sna.Key, CommonPaths.R2RemoveDRMDir + (sna.Key - BaseDirectory), true);

                        // Create the encoder
                        var encoder = new Rayman2SNADataEncoder();

                        // Read the file bytes and decode it
                        var bytes = encoder.Decode(File.ReadAllBytes(sna.Key));

                        // Blank each offset
                        foreach (var offset in sna.Value)
                        {
                            // Blank the value
                            bytes[offset] = 0;
                            bytes[offset + 1] = 0;
                            bytes[offset + 2] = 0;
                            bytes[offset + 3] = 0;
                        }

                        // Encode and write the bytes
                        File.WriteAllBytes(sna.Key, encoder.Encode(bytes));
                    }
                });

                HasBeenApplied = true;

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.R2U_RemoveDRM_Success);
            }
            catch (Exception ex)
            {
                ex.HandleError("Removal R2 DRM");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R2U_RemoveDRM_Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Applies the patch to the current installation
        /// </summary>
        /// <returns>The task</returns>
        public async Task RevertPatchAsync()
        {
            try
            {
                IsLoading = true;

                await Task.Run(() =>
                {
                    // Move back the files
                    RCFRCP.File.MoveFiles(new IOSearchPattern(CommonPaths.R2RemoveDRMDir), BaseDirectory, true);

                    // Delete the backup directory
                    RCFRCP.File.DeleteDirectory(CommonPaths.R2RemoveDRMDir);
                });

                HasBeenApplied = false;

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.R2U_RemoveDRM_RevertSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Reverting R2 DRM patch");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R2U_RemoveDRM_RevertError);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}