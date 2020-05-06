using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Binary;
using RayCarrot.Rayman;

// ReSharper disable StringLiteralTypo

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
                // Open the IPK file
                using var fileStream = File.OpenRead(IPKFilePath);

                // Get the first patch info
                var patchInfo = GetPatchInfos(fileStream).First();

                // Set the position
                fileStream.Position = patchInfo.Offset;

                // Get the byte from that position to see if the patch has been applied
                IsApplied = !fileStream.Read(patchInfo.Patch.OriginalBytes.Length).SequenceEqual(patchInfo.Patch.OriginalBytes);
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting if UbiRay patch has been applied");

                IPKFilePath = FileSystemPath.EmptyPath;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the patches
        /// </summary>
        protected Patch[] GetPatches => new Patch[]
        {
            // This makes UbiRay be treated as a normal costume (type 0)
            new Patch("sgscontainer.ckd", 17841, new byte[]
            {
                0
            }, new byte[]
            {
                1
            }), 

            // This fixes the character description
            new Patch("costumerayman_ubi.act.ckd", 414, GetBytes(6426), GetBytes(5095)), 

            // This fixes the character name
            new Patch("costumerayman_ubi.act.ckd", 418, GetBytes(6425), GetBytes(4876)), 
        };

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
        /// Gets the bytes from a 32-bit integer using big endian
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The bytes</returns>
        protected byte[] GetBytes(int value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>
        /// Gets the patch infos for each <see cref="Patch"/>
        /// </summary>
        /// <param name="ipkStream">The IPK file stream</param>
        /// <returns>The patch infos</returns>
        protected IEnumerable<PatchInfo> GetPatchInfos(Stream ipkStream)
        {
            // Deserialize the IPK file
            var ipk = BinarySerializableHelpers.ReadFromStream<UbiArtIpkData>(ipkStream, UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanLegends, Platform.PC), RCFRCP.App.GetBinarySerializerLogger());

            // Enumerate every patch
            foreach (var patchGroup in GetPatches.GroupBy(x => x.FileName))
            {
                // Get the file
                var file = ipk.Files.FindItem(x => x.Path.FileName == patchGroup.Key);

                // Make sure we found the file
                if (file == null)
                    throw new Exception("Patch file not found");

                // Make sure it's not compressed
                if (file.IsCompressed)
                    throw new Exception("The configuration file is compressed and can not be edited");

                // Get the offsets
                foreach (Patch patch in patchGroup)
                {
                    // Get the offset of the byte in the file to change
                    yield return new PatchInfo(patch, (long)(ipk.BaseOffset + file.Offsets.First() + (uint)patch.FileOffset));
                }
            }
        }

        /// <summary>
        /// Patches the IPK file
        /// </summary>
        /// <param name="usePatchedBytes">Indicates if the patched bytes should be used or if else the original ones should be used</param>
        protected void PatchFile(bool usePatchedBytes)
        {
            // Open the file
            using var fileStream = File.Open(IPKFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

            // Enumerate each patch info
            foreach (PatchInfo patchInfo in GetPatchInfos(fileStream))
            {
                // Set the position
                fileStream.Position = patchInfo.Offset;

                // Modify the bytes
                fileStream.Write(usePatchedBytes ? patchInfo.Patch.PatchedBytes : patchInfo.Patch.OriginalBytes);
            }
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
                PatchFile(true);

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
                PatchFile(false);

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


        #region Classes

        protected class Patch
        {
            public Patch(string fileName, int fileOffset, byte[] patchedBytes, byte[] originalBytes)
            {
                FileName = fileName;
                FileOffset = fileOffset;
                PatchedBytes = patchedBytes;
                OriginalBytes = originalBytes;
            }

            public string FileName { get; }

            public int FileOffset { get; }

            public byte[] PatchedBytes { get; }

            public byte[] OriginalBytes { get; }
        }

        protected class PatchInfo
        {
            public PatchInfo(Patch patch, long offset)
            {
                Patch = patch;
                Offset = offset;
            }

            public Patch Patch { get; }

            public long Offset { get; }
        }

        #endregion
    }
}