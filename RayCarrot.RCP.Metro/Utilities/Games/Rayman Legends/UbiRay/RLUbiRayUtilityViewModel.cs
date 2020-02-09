using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Legends UbiRay utility
    /// </summary>
    public class RLUbiRayUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RLUbiRayUtilityViewModel()
        {
            // Create commands
            ApplyCommand = new AsyncRelayCommand(ApplyAsync);
            RevertCommand = new AsyncRelayCommand(RevertAsync);

            // Set properties
            IPKFilePath = Games.RaymanLegends.GetInstallDir() + "Bundle_PC.ipk";

            try
            {
                // Get the offset
                var offset = GetByteOffset();

                // Open the IPK file
                using var fileStream = File.OpenRead(IPKFilePath);

                // Set the position
                fileStream.Position = offset;

                // Get the byte from that position to see if the patch has been applied
                IsApplied = fileStream.ReadByte() == 0;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting if UbiRay patch has been applied");

                IPKFilePath = FileSystemPath.EmptyPath;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The IPK file path
        /// </summary>
        public FileSystemPath IPKFilePath { get; set; }

        /// <summary>
        /// Indicates if the utility has been applied
        /// </summary>
        public bool IsApplied { get; set; }

        #endregion

        #region Commands

        public ICommand ApplyCommand { get; }

        public ICommand RevertCommand { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the byte offset for where the patch should be applied
        /// </summary>
        /// <returns>The offset</returns>
        protected long GetByteOffset()
        {
            // Deserialize the IPK file
            var ipk = UbiArtIpkData.GetSerializer(UbiArtGameMode.RaymanLegendsPC.GetSettings()).Deserialize(IPKFilePath);

            // Get the file
            var file = ipk.Files.FindItem(x => x.FileName == "sgscontainer.ckd");

            // Make sure we found the file
            if (file == null)
                throw new Exception("Configuration file not found");

            // Make sure it's not compressed
            if (file.IsCompressed)
                throw new Exception("The configuration file is compressed and can not be edited");

            // Get the offset of the byte in the file to change
            return ipk.BaseOffset + file.Offset + 17841;
        }

        /// <summary>
        /// Patches the IPK file
        /// </summary>
        /// <param name="value">The value to patch</param>
        protected void PatchFile(byte value)
        {
            // Get the offset
            var offset = GetByteOffset();

            // Open the file for writing
            using var fileStream = File.OpenWrite(IPKFilePath);

            // Set the position
            fileStream.Position = offset;

            // Modify the byte
            fileStream.WriteByte(value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies the patch
        /// </summary>
        /// <returns>The task</returns>
        public async Task ApplyAsync()
        {
            try
            {
                // Apply the patch
                PatchFile(0);

                RCFCore.Logger?.LogInformationSource($"The Rayman Legends UbiRay utility has been applied");

                IsApplied = true;

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.RLU_UbiRay_ApplySuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying RL UbiRay patch");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex);
            }
        }

        /// <summary>
        /// Reverts the patch
        /// </summary>
        /// <returns>The task</returns>
        public async Task RevertAsync()
        {
            try
            {
                // Apply the patch
                PatchFile(1);

                RCFCore.Logger?.LogInformationSource($"The Rayman Legends UbiRay utility has been reverted");

                IsApplied = false;

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.RLU_UbiRay_RevertSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Reverting RL UbiRay patch");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex);
            }
        }

        #endregion
    }
}