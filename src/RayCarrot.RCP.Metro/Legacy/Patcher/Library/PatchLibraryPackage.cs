﻿#nullable disable
using System.Text;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Legacy.Patcher;

/// <summary>
/// A patch library file (.gpl). This is a custom package file which is stored in the game patch library folder and keeps track
/// of the added and enabled patches as well as the history of files modified by the applied patches.
/// </summary>
public class PatchLibraryPackage : BinarySerializable, IPackageFile
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
    public PatchLibraryPackageHistory History { get; set; }

    /// <summary>
    /// The patches in the library
    /// </summary>
    public PatchLibraryPackagePatchEntry[] Patches { get; set; }

    #endregion

    #region Public Methods

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

            History = s.SerializeObject<PatchLibraryPackageHistory>(History, name: nameof(History));

            Patches = s.SerializeArraySize<PatchLibraryPackagePatchEntry, int>(Patches, name: nameof(Patches));
            Patches = s.SerializeObjectArray<PatchLibraryPackagePatchEntry>(Patches, Patches.Length, name: nameof(Patches));
        });
    }

    #endregion
}