#nullable disable
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A patch file (.gp). This is a custom binary file format which stores the data and resources for a game patch.
/// </summary>
public class PatchFile : BinarySerializable
{
    #region Constants

    public const string FileExtension = ".gp"; // Game Patch
    public const int LatestVersion = 0;

    #endregion

    #region Public Properties

    /// <summary>
    /// The patch file version. This is used for backwards compatibility if the format ever changes.
    /// </summary>
    public int Version { get; set; }

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
    public byte[] Thumbnail { get; set; }

    /// <summary>
    /// The files added or replaced by this patch. These are resources as they contain packed data.
    /// </summary>
    public PatchFileResourceEntry[] AddedFiles { get; set; }

    /// <summary>
    /// The files removed by this patch. This only includes their paths.
    /// </summary>
    public PatchFilePath[] RemovedFiles { get; set; }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Generates a new unique ID for a patch
    /// </summary>
    /// <returns>The generated ID</returns>
    public static string GenerateID() => Guid.NewGuid().ToString();

    /// <summary>
    /// Calculates the checksum for a patch resource
    /// </summary>
    /// <param name="stream">The resource stream</param>
    /// <returns>The calculated checksum as a byte array</returns>
    public static byte[] CalculateChecksum(Stream stream)
    {
        using SHA256Managed sha = new();
        return sha.ComputeHash(stream);
    }

    #endregion

    #region Public Methods

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoWithDefaults(new SerializerDefaults() { StringEncoding = Encoding.UTF8 }, () =>
        {
            s.SerializeMagicString("GP", 2);
            Version = s.Serialize<int>(Version, name: nameof(Version));

            if (Version > LatestVersion)
                throw new UnsupportedFormatVersionException($"The patch version {Version} is higher than the latest supported version {LatestVersion}");

            Metadata = s.SerializeObject<PatchMetadata>(Metadata, name: nameof(Metadata));

            HasThumbnail = s.Serialize<bool>(HasThumbnail, name: nameof(HasThumbnail));

            if (HasThumbnail)
            {
                Thumbnail = s.SerializeArraySize<byte, int>(Thumbnail, name: nameof(Thumbnail));
                Thumbnail = s.SerializeArray<byte>(Thumbnail, Thumbnail.Length, name: nameof(Thumbnail));
            }

            AddedFiles = s.SerializeArraySize<PatchFileResourceEntry, int>(AddedFiles, name: nameof(AddedFiles));
            AddedFiles = s.SerializeObjectArray<PatchFileResourceEntry>(AddedFiles, AddedFiles.Length, name: nameof(AddedFiles));

            RemovedFiles = s.SerializeArraySize<PatchFilePath, int>(RemovedFiles, name: nameof(RemovedFiles));
            RemovedFiles = s.SerializeObjectArray<PatchFilePath>(RemovedFiles, RemovedFiles.Length, name: nameof(RemovedFiles));
        });
    }

    #endregion
}