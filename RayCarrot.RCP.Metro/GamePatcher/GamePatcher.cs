using System;
using System.IO;
using System.Linq;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;

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
        /// <param name="originalBytes">The original bytes to patch</param>
        /// <param name="patchedBytes">The bytes to replace the original ones with</param>
        /// <param name="offset">The byte offset</param>
        public GamePatcher(FileSystemPath gameFile, byte[] originalBytes, byte[] patchedBytes, int offset)
        {
            GameFile = gameFile;
            OriginalBytes = originalBytes;
            PatchedBytes = patchedBytes;
            Offset = offset;

            if (originalBytes.Length != patchedBytes.Length)
                throw new ArgumentException("The original and patched bytes need to be of the same length");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game file to patch
        /// </summary>
        public FileSystemPath GameFile { get; }

        /// <summary>
        /// The original bytes to patch
        /// </summary>
        public byte[] OriginalBytes { get; }

        /// <summary>
        /// The bytes to replace the original ones with
        /// </summary>
        public byte[] PatchedBytes { get; }

        /// <summary>
        /// The byte offset
        /// </summary>
        public int Offset { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the game is not patched
        /// </summary>
        /// <returns>True if the game is not patched or false if it is. Null is returned if the game file can not be found or if neither the original or patched bytes are found.</returns>
        public bool? GetIsOriginal()
        {
            RCFCore.Logger?.LogInformationSource("Getting if game file is patched or original...");

            try
            {
                // Create a buffer
                byte[] currentBytes = new byte[OriginalBytes.Length];

                // Open the file as a stream
                using (Stream stream = File.Open(GameFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // Set the position
                    stream.Position = Offset;

                    // Read the bytes
                    var read = stream.Read(currentBytes, 0, currentBytes.Length);

                    RCFCore.Logger?.LogInformationSource($"{read}/{currentBytes.Length} bytes were read from the game file");
                }

                // Check if they match
                if (currentBytes.SequenceEqual(OriginalBytes))
                {
                    RCFCore.Logger?.LogInformationSource("The game file was detected as original");
                    return true;
                }
                else if (currentBytes.SequenceEqual(PatchedBytes))
                {
                    RCFCore.Logger?.LogInformationSource("The game file was detected as patched");
                    return false;
                }
                else
                {
                    RCFCore.Logger?.LogWarningSource("The game file was detected as unknown");
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
            RCFCore.Logger?.LogInformationSource("Patching game file...");

            try
            {
                // Open the file as a stream
                using (Stream stream = File.Open(GameFile, FileMode.Open, FileAccess.Write))
                {
                    // Set the position
                    stream.Position = Offset;

                    // Write the bytes
                    stream.Write(useOriginalBytes ? OriginalBytes : PatchedBytes, 0, OriginalBytes.Length);
                }

                RCFCore.Logger?.LogInformationSource("Game file was patched");
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