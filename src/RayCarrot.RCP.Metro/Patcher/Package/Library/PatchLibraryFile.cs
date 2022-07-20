#nullable disable
using System;
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
    public const int LatestFormatVersion = 0;

    #endregion

    #region Public Properties

    public IEnumerable<PackagedResourceEntry> Resources => History.RemovedFileResources.Concat(History.ReplacedFileResources);

    /// <summary>
    /// The library file format version. This is used for backwards compatibility if the format ever changes.
    /// </summary>
    public int FormatVersion { get; set; }

    private string GameName { get; set; }

    /// <summary>
    /// The game this patch library is used for
    /// </summary>
    public Games Game
    {
        get => (Games)Enum.Parse(typeof(Games), GameName);
        set => GameName = value.ToString();
    }

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

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoWithDefaults(new SerializerDefaults() { StringEncoding = Encoding.UTF8 }, () =>
        {
            s.SerializeMagicString("GPL", 4);
            FormatVersion = s.Serialize<int>(FormatVersion, name: nameof(FormatVersion));

            if (FormatVersion > LatestFormatVersion)
                throw new UnsupportedFormatVersionException($"The library format version {FormatVersion} is higher than the latest supported version {LatestFormatVersion}");

            GameName = s.SerializeString(GameName, name: nameof(GameName));

            History = s.SerializeObject<PatchLibraryHistory>(History, name: nameof(History));

            Patches = s.SerializeArraySize<PatchLibraryPatchEntry, int>(Patches, name: nameof(Patches));
            Patches = s.SerializeObjectArray<PatchLibraryPatchEntry>(Patches, Patches.Length, name: nameof(Patches));
        });
    }

    #endregion
}