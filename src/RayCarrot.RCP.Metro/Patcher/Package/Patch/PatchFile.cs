#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using BinarySerializer;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A patch file (.gp). This is a custom binary file format which stores the data and resources for a game patch.
/// </summary>
public class PatchFile : BinarySerializable, IPackageFile
{
    #region Constants

    private const string FileTypeID = "RCP_Metro.GamePatch";
    private const string URIProtocol = "rcpgp";

    public const string FileExtension = ".gp"; // Game Patch
    public const int LatestFormatVersion = 0;

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public IEnumerable<PackagedResourceEntry> Resources
    {
        get
        {
            foreach (PackagedResourceEntry resource in AddedFileResources)
                yield return resource;

            if (HasThumbnail)
                yield return ThumbnailResource;
        }
    }

    /// <summary>
    /// The patch file format version. This is used for backwards compatibility if the format ever changes.
    /// </summary>
    public int FormatVersion { get; set; }

    /// <summary>
    /// The patch metadata containing general information about the patch
    /// </summary>
    public PatchMetadata Metadata { get; set; }

    /// <summary>
    /// Indicates if the patch has a thumbnail or not
    /// </summary>
    public bool HasThumbnail { get; set; }

    /// <summary>
    /// The patch thumbnail or <see langword="null" /> if <see cref="HasThumbnail"/> is false. As of version 0
    /// this is always expected to be a a PNG file.
    /// </summary>
    public PackagedResourceEntry ThumbnailResource { get; set; }

    /// <summary>
    /// The files added or replaced by this patch
    /// </summary>
    public PatchFilePath[] AddedFiles { get; set; }

    /// <summary>
    /// The checksums for the added files
    /// </summary>
    public PackagedResourceChecksum[] AddedFileChecksums { get; set; }

    /// <summary>
    /// The resources for the added files
    /// </summary>
    public PackagedResourceEntry[] AddedFileResources { get; set; }

    /// <summary>
    /// The files removed by this patch
    /// </summary>
    public PatchFilePath[] RemovedFiles { get; set; }

    #endregion

    #region Public Static Methods

    public static bool? IsAssociatedWithFileType()
    {
        try
        {
            return WindowsHelpers.GetFileTypeAssociationID(FileExtension) == FileTypeID;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if the patch file type association is set");
            return null;
        }
    }

    public static bool? IsAssociatedWithURIProtocol()
    {
        try
        {
            return WindowsHelpers.GetHasURIProtocolAssociation(URIProtocol);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if the patch URI protocol association is set");
            return null;
        }
    }

    public static void AssociateWithFileType(FileSystemPath programFilePath, bool enable)
    {
        WindowsHelpers.SetFileTypeAssociation(programFilePath, FileExtension, "Rayman Control Panel Game Patch", FileTypeID, enable);
        
        if (enable)
            Logger.Info("Set the file type association for patch files");
        else
            Logger.Info("Removed the file type association for patch files");
    }

    public static void AssociateWithURIProtocol(FileSystemPath programFilePath, bool enable)
    {
        WindowsHelpers.SetURIProtocolAssociation(programFilePath, URIProtocol, "Rayman Control Panel Game Patch Protocol", enable);

        if (enable)
            Logger.Info("Set the URI protocol association for patch files");
        else
            Logger.Info("Removed the URI protocol association for patch files");
    }

    #endregion

    #region Public Methods

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoWithDefaults(new SerializerDefaults() { StringEncoding = Encoding.UTF8 }, () =>
        {
            s.SerializeMagicString("GP", 4);
            FormatVersion = s.Serialize<int>(FormatVersion, name: nameof(FormatVersion));

            if (FormatVersion > LatestFormatVersion)
                throw new UnsupportedFormatVersionException(this, $"The patch format version {FormatVersion} is higher than the latest supported version {LatestFormatVersion}");

            Metadata = s.SerializeObject<PatchMetadata>(Metadata, name: nameof(Metadata));

            HasThumbnail = s.Serialize<bool>(HasThumbnail, name: nameof(HasThumbnail));

            if (HasThumbnail)
                ThumbnailResource = s.SerializeObject<PackagedResourceEntry>(ThumbnailResource, name: nameof(ThumbnailResource));

            AddedFiles = s.SerializeArraySize<PatchFilePath, int>(AddedFiles, name: nameof(AddedFiles));
            AddedFiles = s.SerializeObjectArray<PatchFilePath>(AddedFiles, AddedFiles.Length, name: nameof(AddedFiles));

            AddedFileChecksums = s.SerializeArraySize<PackagedResourceChecksum, int>(AddedFileChecksums, name: nameof(AddedFileChecksums));
            AddedFileChecksums = s.SerializeObjectArray<PackagedResourceChecksum>(AddedFileChecksums, AddedFileChecksums.Length, name: nameof(AddedFileChecksums));

            AddedFileResources = s.SerializeArraySize<PackagedResourceEntry, int>(AddedFileResources, name: nameof(AddedFileResources));
            AddedFileResources = s.SerializeObjectArray<PackagedResourceEntry>(AddedFileResources, AddedFileResources.Length, name: nameof(AddedFileResources));

            if (AddedFiles.Length != AddedFileChecksums.Length || AddedFiles.Length != AddedFileResources.Length)
                throw new BinarySerializableException(this, $"The added file array lengths don't match ({AddedFiles.Length}, {AddedFileChecksums.Length}, {AddedFileResources.Length})");

            RemovedFiles = s.SerializeArraySize<PatchFilePath, int>(RemovedFiles, name: nameof(RemovedFiles));
            RemovedFiles = s.SerializeObjectArray<PatchFilePath>(RemovedFiles, RemovedFiles.Length, name: nameof(RemovedFiles));
        });
    }

    #endregion
}