#nullable disable
using System;
using System.IO;
using System.Linq;
using RayCarrot.IO;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Handles patching for game files
/// </summary>
public class FilePatcher
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameFile">The game file to patch</param>
    /// <param name="patches">The patches to use</param>
    public FilePatcher(FileSystemPath gameFile, FilePatcher_Patch[] patches)
    {
        GameFile = gameFile;
        Patches = patches;

        if (Patches.Select(x => x.FileSize).Distinct().Count() != Patches.Length)
            throw new ArgumentException("All patches must have unique file sizes", nameof(patches));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The game file to patch
    /// </summary>
    public FileSystemPath GameFile { get; }

    /// <summary>
    /// The patches to use
    /// </summary>
    public FilePatcher_Patch[] Patches { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Checks if the game is not patched
    /// </summary>
    /// <returns>True if the game is not patched or false if it is. Null is returned if the game file can not be found or if neither the original or patched bytes are found.</returns>
    public bool? GetIsOriginal()
    {
        Logger.Info("Getting if game file is patched or original");

        try
        {
            // Get the file size
            var fileSize = (uint)GameFile.GetSize().Bytes;

            // Find matching patch
            var patch = Patches.FirstOrDefault(x => x.FileSize == fileSize);

            if (patch == null)
            {
                Logger.Warn("The game file size does not match any available patch");
                return null;
            }

            // Check the first patch entry
            FilePatcher_Patch.PatchEntry patchEntry = patch.PatchEntries[0];

            // Create a buffer
            byte[] currentBytes = new byte[patchEntry.OriginalBytes.Length];

            // Open the file as a stream
            using (Stream stream = File.Open(GameFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // Set the position
                stream.Position = patchEntry.PatchOffset;

                // Read the bytes
                var read = stream.Read(currentBytes, 0, currentBytes.Length);

                Logger.Info("{0}/{1} bytes were read from the game file", read, currentBytes.Length);
            }

            // Check if they match
            if (currentBytes.SequenceEqual(patchEntry.OriginalBytes))
            {
                Logger.Info("The game file was detected as original");
                return true;
            }
            else if (currentBytes.SequenceEqual(patchEntry.PatchedBytes))
            {
                Logger.Info("The game file was detected as patched");
                return false;
            }
            else
            {
                Logger.Warn("The game file was detected as unknown");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting if game is patched");
            return null;
        }
    }

    /// <summary>
    /// Patches the game
    /// </summary>
    /// <param name="useOriginalBytes">True if the original bytes should be used, otherwise false</param>
    public void PatchFile(bool useOriginalBytes)
    {
        Logger.Info("Patching game file...");

        try
        {
            // Open the file as a stream
            using Stream stream = File.Open(GameFile, FileMode.Open, FileAccess.Write);

            // Find matching patch
            var patch = Patches.First(x => x.FileSize == stream.Length);

            // Apply each patch entry
            foreach (FilePatcher_Patch.PatchEntry patchEntry in patch.PatchEntries)
            {
                // Set the position
                stream.Position = patchEntry.PatchOffset;

                // Write the bytes
                stream.Write(useOriginalBytes ? patchEntry.OriginalBytes : patchEntry.PatchedBytes, 0, patchEntry.OriginalBytes.Length);
            }

            Logger.Info("Game file was patched");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Patching game file");
            throw;
        }
    }

    #endregion
}