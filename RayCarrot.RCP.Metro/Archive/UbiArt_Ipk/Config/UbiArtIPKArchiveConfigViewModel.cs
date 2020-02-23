using System;
using System.Collections.Generic;
using System.Linq;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Configuration view model for .ipk archives
    /// </summary>
    public class UbiArtIPKArchiveConfigViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        /// <param name="compressionMode">The compression mode for files and the block</param>
        public UbiArtIPKArchiveConfigViewModel(UbiArtSettings settings, FileCompressionMode compressionMode)
        {
            CompressionMode = compressionMode;
            Settings = settings;
            CompressedExtensions = ".dtape.ckd," +
                                   ".fx.fxb," +
                                   ".m3d.ckd," +
                                   ".png.ckd," +
                                   ".tga.ckd";
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        public UbiArtSettings Settings { get; }

        /// <summary>
        /// The compression mode for files and the block
        /// </summary>
        public FileCompressionMode CompressionMode { get; }

        /// <summary>
        /// The file extensions to compress, separated by a comma
        /// </summary>
        public string CompressedExtensions { get; set; }

        /// <summary>
        /// Indicates if the entire block can be compressed
        /// </summary>
        public bool CanCompressEntireBlock
        {
            get
            {
                // Create dummy data
                var dummyData = new UbiArtIpkData();

                // Configure it...
                ConfigureIpkData(dummyData);

                return dummyData.SupportsCompressedBlock;
            }
        }

        /// <summary>
        /// Indicates if the entire block should be compressed
        /// </summary>
        public bool CompressEntireBlock { get; set; }

        /// <summary>
        /// The file extensions to compress
        /// </summary>
        public IEnumerable<FileExtension> CompressedFileExtensions => CompressedExtensions?.Split(',').Select(x => new FileExtension(x));

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the .ipk data with the default settings for the current settings
        /// </summary>
        /// <param name="data">The .ipk data to configure</param>
        public void ConfigureIpkData(UbiArtIpkData data)
        {
            // Set default properties based on settings
            switch (Settings.Game)
            {
                case UbiArtGame.RaymanOrigins:

                    if (Settings.Platform != UbiArtPlatform.Nintendo3DS)
                    {
                        data.Version = 3;
                        data.Unknown1 = 0;
                    }
                    else
                    {
                        data.Version = 4;
                        data.Unknown1 = 5;
                    }

                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 0;

                    break;

                case UbiArtGame.RaymanLegends:

                    if (Settings.Platform != UbiArtPlatform.PlayStation4)
                    {
                        data.Version = 5;
                        data.Unknown1 = 0;
                    }
                    else
                    {
                        data.Version = 7;
                        data.Unknown1 = 8;
                    }

                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 30765;

                    break;

                case UbiArtGame.RaymanAdventures:

                    data.Version = 8;
                    data.Unknown1 = 2;
                    data.Unknown2 = 11;
                    data.Unknown3 = true;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 30765;

                    break;

                case UbiArtGame.RaymanMini:

                    data.Version = 8;
                    data.Unknown1 = 12;
                    data.Unknown2 = 12;
                    data.Unknown3 = true;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 3826;
                    data.EngineVersion = 3826;

                    break;

                case UbiArtGame.JustDance2017:

                    data.Version = 5;
                    data.Unknown1 = 8;
                    data.Unknown3 = false;
                    data.Unknown4 = false;
                    data.Unknown5 = false;
                    data.Unknown6 = 0;
                    data.EngineVersion = 241478;

                    break;

                case UbiArtGame.ValiantHearts:

                    data.Version = 7;
                    data.Unknown1 = 10;
                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 0;
                    data.Unknown9 = 0;

                    break;

                case UbiArtGame.ChildOfLight:

                    data.Version = 7;
                    data.Unknown1 = 0;
                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 30765;

                    break;

                case UbiArtGame.GravityFalls:

                    data.Version = 7;
                    data.Unknown1 = 10;
                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 0;

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Settings.Game), Settings.Game, null);
            }

            // Unknown value used for all versions which we set to 0
            data.Unknown7 = 0;
        }

        /// <summary>
        /// Gets a value indicating if the file should be compressed
        /// </summary>
        /// <param name="entry">The file entry to check</param>
        /// <returns>True if the file should be compressed, otherwise false</returns>
        public bool ShouldCompress(UbiArtIPKFileEntry entry)
        {
            return CompressionMode switch
            {
                FileCompressionMode.Never => false,
                FileCompressionMode.Always => true,

                FileCompressionMode.MatchesSetting => !CompressEntireBlock && CompressedFileExtensions.Any(x => x == new FileExtension(entry.Path.FileName)),
                FileCompressionMode.WasCompressed => entry.IsCompressed,

                _ => throw new ArgumentOutOfRangeException(nameof(CompressionMode), CompressionMode, null)
            };
        }

        /// <summary>
        /// Gets a value indicating if the data block should be compressed
        /// </summary>
        /// <param name="data">The .ipk data to check</param>
        /// <returns>True if the data block should be compressed, otherwise false</returns>
        public bool ShouldCompress(UbiArtIpkData data)
        {
            return CompressionMode switch
            {
                FileCompressionMode.Never => false,
                FileCompressionMode.Always => true,

                FileCompressionMode.MatchesSetting => CompressEntireBlock,
                FileCompressionMode.WasCompressed => data.IsBlockCompressed,

                _ => throw new ArgumentOutOfRangeException(nameof(CompressionMode), CompressionMode, null)
            };
        }

        #endregion

        #region Enums

        /// <summary>
        /// The available file compression modes
        /// </summary>
        public enum FileCompressionMode
        {
            /// <summary>
            /// Files are never compressed
            /// </summary>
            Never,

            /// <summary>
            /// Files are always compressed
            /// </summary>
            Always,

            /// <summary>
            /// Files which match the formats set to be compressed are compressed
            /// </summary>
            MatchesSetting,

            /// <summary>
            /// Files which were previously compressed are compressed
            /// </summary>
            WasCompressed
        }

        #endregion
    }
}