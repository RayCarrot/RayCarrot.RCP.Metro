using System.IO;

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

        if (Patches.SelectMany(x => x.FileSizes).Distinct().Count() != Patches.Sum(x => x.FileSizes.Length))
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
    /// Retrieves the patch state
    /// </summary>
    /// <returns>The patch state</returns>
    public PatchState GetPatchState()
    {
        Logger.Info("Getting if game file is patched or original");

        try
        {
            // Get the file size
            uint fileSize = (uint)GameFile.GetSize();

            // Find matching patch
            int patchIndex = Patches.FindItemIndex(x => x.FileSizes.Contains(fileSize));

            if (patchIndex == -1)
            {
                Logger.Warn("The game file size does not match any available patch");
                return null;
            }

            FilePatcher_Patch patch = Patches[patchIndex];

            // IDEA: Check all patch entries?
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
                int read = stream.Read(currentBytes, 0, currentBytes.Length);

                Logger.Info("{0}/{1} bytes were read from the game file", read, currentBytes.Length);
            }

            // Check if they match the original
            if (currentBytes.SequenceEqual(patchEntry.OriginalBytes))
            {
                Logger.Info("The game file was detected as original");
                return new PatchState(patchIndex, false);
            }

            // Check if it's patches, checking revisions starting with newest first
            bool isOutdated = false;
            foreach (FilePatcher_Patch.PatchedBytesRevision revision in patchEntry.PatchRevisions.OrderByDescending(x => x.Version))
            {
                if (currentBytes.SequenceEqual(revision.Bytes))
                {
                    Logger.Info($"The game file was detected as patched with version {revision.Version}");
                    return new PatchState(patchIndex, true, revision.Version, isOutdated);
                }

                isOutdated = true;
            }

            Logger.Warn("The game file patch state could not be determined");
            return null;
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
    /// <param name="applyPatch">True if the latest patch should be applied or false to revert the patch</param>
    public void PatchFile(bool applyPatch)
    {
        Logger.Info("Patching game file...");

        try
        {
            // Open the file as a stream
            using Stream stream = File.Open(GameFile, FileMode.Open, FileAccess.Write);

            // Find matching patch
            FilePatcher_Patch patch = Patches.First(x => x.FileSizes.Contains((uint)stream.Length));

            // Apply each patch entry
            foreach (FilePatcher_Patch.PatchEntry patchEntry in patch.PatchEntries)
            {
                // Set the position
                stream.Position = patchEntry.PatchOffset;

                byte[] bytes;

                if (applyPatch)
                    bytes = patchEntry.PatchRevisions.OrderBy(x => x.Version).Last().Bytes;
                else
                    bytes = patchEntry.OriginalBytes;

                // Write the bytes
                stream.Write(bytes, 0, patchEntry.OriginalBytes.Length);
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

    #region Records

    public record PatchState(int PatchIndex, bool IsPatched, int PatchVersion = -1, bool IsVersionOutdated = false);

    #endregion
}