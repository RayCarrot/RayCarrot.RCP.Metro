using System;
using System.IO;
using System.Linq;
using RayCarrot.IO;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles patching for game executable files
    /// </summary>
    public class GamePatcher
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="gameFile">The game file to patch</param>
        /// <param name="patches">The patches to use</param>
        public GamePatcher(FileSystemPath gameFile, GamePatcherData[] patches)
        {
            GameFile = gameFile;
            Patches = patches;

            if (Patches.Select(x => x.FileSize).Distinct().Count() != Patches.Length)
                throw new ArgumentException("All patches must have unique file sizes", nameof(patches));
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game file to patch
        /// </summary>
        public FileSystemPath GameFile { get; }

        /// <summary>
        /// The patches to use
        /// </summary>
        public GamePatcherData[] Patches { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the game is not patched
        /// </summary>
        /// <returns>True if the game is not patched or false if it is. Null is returned if the game file can not be found or if neither the original or patched bytes are found.</returns>
        public bool? GetIsOriginal()
        {
            RL.Logger?.LogInformationSource("Getting if game file is patched or original...");

            try
            {
                // Get the file size
                var fileSize = (uint)GameFile.GetSize().Bytes;

                // Find matching patch
                var patch = Patches.FirstOrDefault(x => x.FileSize == fileSize);

                if (patch == null)
                {
                    RL.Logger?.LogWarningSource("The game file size does not match any available patch");
                    return null;
                }

                // Create a buffer
                byte[] currentBytes = new byte[patch.OriginalBytes.Length];

                // Open the file as a stream
                using (Stream stream = File.Open(GameFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // Set the position
                    stream.Position = patch.PatchOffset;

                    // Read the bytes
                    var read = stream.Read(currentBytes, 0, currentBytes.Length);

                    RL.Logger?.LogInformationSource($"{read}/{currentBytes.Length} bytes were read from the game file");
                }

                // Check if they match
                if (currentBytes.SequenceEqual(patch.OriginalBytes))
                {
                    RL.Logger?.LogInformationSource("The game file was detected as original");
                    return true;
                }
                else if (currentBytes.SequenceEqual(patch.PatchedBytes))
                {
                    RL.Logger?.LogInformationSource("The game file was detected as patched");
                    return false;
                }
                else
                {
                    RL.Logger?.LogWarningSource("The game file was detected as unknown");
                    return null;
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting if game is patched");
                return null;
            }
        }

        /// <summary>
        /// Patches the game
        /// </summary>
        /// <param name="useOriginalBytes">True if the original bytes should be used, otherwise false</param>
        public void PatchFile(bool useOriginalBytes)
        {
            RL.Logger?.LogInformationSource("Patching game file...");

            try
            {
                // Open the file as a stream
                using Stream stream = File.Open(GameFile, FileMode.Open, FileAccess.Write);

                // Find matching patch
                var patch = Patches.First(x => x.FileSize == stream.Length);

                // Set the position
                stream.Position = patch.PatchOffset;

                // Write the bytes
                stream.Write(useOriginalBytes ? patch.OriginalBytes : patch.PatchedBytes, 0, patch.OriginalBytes.Length);

                RL.Logger?.LogInformationSource("Game file was patched");
            }
            catch (Exception ex)
            {
                ex.HandleError("Patching game file");
                throw;
            }
        }

        #endregion
    }
}