#nullable disable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A patch library file (.gpl). This is a custom package file which is stored in the game patch library folder and keeps track
/// of the added and enabled patches as well as the history of files modified by the applied patches.
/// </summary>
public class PatchLibraryFile : BinarySerializable, IPackageFile
{
    #region Constants

    public const string FileExtension = ".gpl"; // Game Patch Library
    public const int LatestFormatVersion = 1;

    #endregion

    #region Public Properties

    public IEnumerable<PackagedResourceEntry> Resources => History.RemovedFileResources.Concat(History.ReplacedFileResources);

    /// <summary>
    /// The library file format version. This is used for backwards compatibility if the format ever changes.
    /// </summary>
    public int FormatVersion { get; set; }

    private string LegacyGameName { get; set; }
    public string GameId { get; set; }

    /// <summary>
    /// The history of the files modified by applied patches
    /// </summary>
    public PatchLibraryHistory History { get; set; }

    /// <summary>
    /// The patches in the library
    /// </summary>
    public PatchLibraryPatchEntry[] Patches { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Checks if the game descriptor is a valid game for this patch library
    /// </summary>
    /// <param name="gameDescriptor"></param>
    /// <returns></returns>
    public bool IsGameValid(GameDescriptor gameDescriptor)
    {
        if (FormatVersion >= 1)
            return gameDescriptor.Id == GameId;
        else
            return gameDescriptor.LegacyGame.ToString() == LegacyGameName;
    }

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoWithDefaults(new SerializerDefaults() { StringEncoding = Encoding.UTF8 }, () =>
        {
            s.SerializeMagicString("GPL", 4);
            FormatVersion = s.Serialize<int>(FormatVersion, name: nameof(FormatVersion));

            if (FormatVersion > LatestFormatVersion)
                throw new UnsupportedFormatVersionException(this, $"The library format version {FormatVersion} is higher than the latest supported version {LatestFormatVersion}");

            // Starting with version 14.0 of RCP games are now identified by the descriptor id
            if (FormatVersion >= 1)
                GameId = s.SerializeString(GameId, name: nameof(GameId));
            else
                LegacyGameName = s.SerializeString(LegacyGameName, name: nameof(LegacyGameName));

            History = s.SerializeObject<PatchLibraryHistory>(History, name: nameof(History));

            Patches = s.SerializeArraySize<PatchLibraryPatchEntry, int>(Patches, name: nameof(Patches));
            Patches = s.SerializeObjectArray<PatchLibraryPatchEntry>(Patches, Patches.Length, name: nameof(Patches));
        });
    }

    #endregion
}