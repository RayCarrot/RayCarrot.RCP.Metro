using System.Collections.Generic;
using System.IO;
using RayCarrot.Binary;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base archive data manager for an OpenSpace .cnt file
    /// </summary>
    public abstract class BaseOpenSpaceCntArchiveDataManager : IArchiveBaseDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        protected BaseOpenSpaceCntArchiveDataManager(OpenSpaceSettings settings)
        {
            Settings = settings;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected OpenSpaceSettings Settings { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        public char PathSeparatorCharacter => '\\';

        /// <summary>
        /// The file filter to use for the archive files
        /// </summary>
        public string ArchiveFileFilter => new FileFilterItem("*.cnt", "CNT").ToString();

        #endregion

        #region Public Methods

        /// <summary>
        /// Encodes the file bytes
        /// </summary>
        /// <param name="fileData">The bytes to encode</param>
        /// <param name="fileEntry">The file entry for the file to encode</param>
        /// <returns>The encoded bytes</returns>
        public byte[] EncodeFile(byte[] fileData, object fileEntry)
        {
            // Get the file entry
            var file = fileEntry.CastTo<OpenSpaceCntFileEntry>();

            // Update the size
            file.Size = fileData.Length;

            // Remove the encryption
            file.FileXORKey = new byte[]
            {
                0, 0, 0, 0
            };

            // Return the data
            return fileData;
        }

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The import data for the archive files</param>
        public void UpdateArchive(object archive, Stream outputFileStream, IEnumerable<IArchiveImportData> files)
        {
            RL.Logger?.LogInformationSource($"A CNT archive is being repacked...");

            // Get the archive data
            var data = archive.CastTo<OpenSpaceCntData>();

            // Create the file generator
            using var fileGenerator = new ArchiveFileGenerator<OpenSpaceCntFileEntry>();

            // Set the current pointer position to the header size
            var pointer = data.GetHeaderSize(Settings);

            // Disable checksum
            data.IsChecksumUsed = false;
            data.DirChecksum = 0;

            // NOTE: We can't disable the XOR key entirely as that would disable it for the file bytes too, which would require them all to be decrypted
            // Reset XOR keys
            data.XORKey = 0;

            // Load each file
            foreach (var importData in files)
            {
                // Get the file entry
                var file = importData.FileEntryData.CastTo<OpenSpaceCntFileEntry>();

                // Reset checksum and XOR key
                file.Checksum = 0;
                file.XORKey = 0;

                // Add to the generator
                fileGenerator.Add(file, () =>
                {
                    // Get the file bytes to write to the archive
                    var bytes = importData.GetData(file);

                    // Set the pointer
                    file.Pointer = pointer;

                    // Update the pointer by the file size
                    pointer += file.Size;

                    return bytes;
                });
            }

            // Write the files
            data.WriteArchiveContent(outputFileStream, fileGenerator);

            outputFileStream.Position = 0;

            // Serialize the data
            BinarySerializableHelpers.WriteToStream(data, outputFileStream, Settings, RCPServices.App.GetBinarySerializerLogger());

            RL.Logger?.LogInformationSource($"The CNT archive has been repacked");
        }

        #endregion
    }
}