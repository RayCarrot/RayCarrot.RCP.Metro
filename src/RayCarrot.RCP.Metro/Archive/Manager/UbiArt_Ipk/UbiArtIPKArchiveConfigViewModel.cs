﻿using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro.Archive.UbiArt;

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
            BundleBootHeader dummyData = new();

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
    public IEnumerable<FileExtension> CompressedFileExtensions => CompressedExtensions.Split(',').Select(x => new FileExtension(x, multiple: true));

    #endregion

    #region Public Methods

    /// <summary>
    /// Configures the .ipk data with the default settings for the current settings
    /// </summary>
    /// <param name="data">The .ipk data to configure</param>
    public void ConfigureIpkData(BundleBootHeader data)
    {
        // Set default properties based on settings
        switch (Settings.Game)
        {
            case BinarySerializer.UbiArt.Game.RaymanOrigins:

                switch (Settings.Platform)
                {
                    case Platform.Wii:
                        data.Version = 3;
                        data.Unknown1 = 6;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 1698768603;
                        data.EngineVersion = 0;
                        break;

                    case Platform.Nintendo3DS:
                        data.Version = 4;
                        data.Unknown1 = 5;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 1635089726;
                        data.EngineVersion = 0;
                        break;

                    case Platform.PlayStation3:
                        data.Version = 3;
                        data.Unknown1 = 3;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 1698768603;
                        data.EngineVersion = 0;
                        break;

                    case Platform.PSVita:
                        data.Version = 3;
                        data.Unknown1 = 7;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 559042371;
                        data.EngineVersion = 0;
                        break;

                    case Platform.Xbox360:
                        data.Version = 3;
                        data.Unknown1 = 1;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 1698768603;
                        data.EngineVersion = 0;
                        break;

                    case Platform.PC:
                        data.Version = 3;
                        data.Unknown1 = 0;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 877930951;
                        data.EngineVersion = 0;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Settings.Platform), Settings.Platform, null);
                }

                break;

            case BinarySerializer.UbiArt.Game.RaymanLegends:

                switch (Settings.Platform)
                {
                    case Platform.WiiU:
                        data.Version = 5;
                        data.Unknown1 = 6;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 78992;
                        data.Unknown7 = 2697850994;
                        data.EngineVersion = 84435;
                        break;

                    case Platform.NintendoSwitch:
                        data.Version = 7;
                        data.Unknown1 = 10;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 2514498303;
                        data.EngineVersion = 0;
                        break;

                    case Platform.PSVita:
                        data.Version = 5;
                        data.Unknown1 = 6;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 2869177618;
                        data.EngineVersion = 0;
                        break;

                    case Platform.PlayStation3:
                        data.Version = 5;
                        data.Unknown1 = 2;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 79403;
                        data.Unknown7 = 410435206;
                        data.EngineVersion = 86846;
                        break;

                    case Platform.PlayStation4:
                        data.Version = 7;
                        data.Unknown1 = 8;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 80253;
                        data.Unknown7 = 2973796970;
                        data.EngineVersion = 117321;
                        break;

                    case Platform.Xbox360:
                        data.Version = 5;
                        data.Unknown1 = 1;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 410435206;
                        data.EngineVersion = 0;
                        break;

                    case Platform.XboxOne:
                        data.Version = 7;
                        data.Unknown1 = 9;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 80262;
                        data.Unknown7 = 2973796970;
                        data.EngineVersion = 118166;
                        break;

                    case Platform.PC:
                        data.Version = 5;
                        data.Unknown1 = 0;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 1274838019;
                        data.EngineVersion = 0;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Settings.Platform), Settings.Platform, null);
                }

                break;

            case BinarySerializer.UbiArt.Game.RaymanAdventures:

                switch (Settings.Platform)
                {
                    case Platform.Android:
                        data.Version = 8;
                        data.Unknown1 = 12;
                        data.Unknown2 = 11;
                        data.Unknown3 = true;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 127901;
                        data.Unknown7 = 3037303110;
                        data.EngineVersion = 277220;
                        break;

                    case Platform.iOS:
                        data.Version = 8;
                        data.Unknown1 = 12;
                        data.Unknown2 = 19;
                        data.Unknown3 = true;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 127895;
                        data.Unknown7 = 3037303110;
                        data.EngineVersion = 277216;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Settings.Platform), Settings.Platform, null);
                }

                break;

            case BinarySerializer.UbiArt.Game.RaymanMini:

                switch (Settings.Platform)
                {
                    case Platform.Mac:
                        data.Version = 8;
                        data.Unknown1 = 12;
                        data.Unknown2 = 11;
                        data.Unknown3 = true;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 4533;
                        data.Unknown7 = 2293139714;
                        data.EngineVersion = 4533;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Settings.Platform), Settings.Platform, null);
                }

                break;

            case BinarySerializer.UbiArt.Game.JustDance2017:

                switch (Settings.Platform)
                {
                    case Platform.WiiU:
                        data.Version = 5;
                        data.Unknown1 = 8;
                        data.Unknown2 = 0;
                        data.Unknown3 = false;
                        data.Unknown4 = false;
                        data.Unknown5 = false;
                        data.Unknown6 = 0;
                        data.Unknown7 = 3346979248;
                        data.EngineVersion = 241478;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Settings.Platform), Settings.Platform, null);
                }

                break;

            case BinarySerializer.UbiArt.Game.ValiantHearts:

                switch (Settings.Platform)
                {
                    case Platform.Android:
                        data.Version = 7;
                        data.Unknown1 = 10;
                        data.Unknown2 = 0;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown9 = 0;
                        data.Unknown7 = 3713665533;
                        data.EngineVersion = 0;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Settings.Platform), Settings.Platform, null);
                }

                break;

            case BinarySerializer.UbiArt.Game.ChildOfLight:

                switch (Settings.Platform)
                {
                    // NOTE: This is based on the demo
                    case Platform.PC:
                        data.Version = 7;
                        data.Unknown1 = 0;
                        data.Unknown2 = 0;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 3669482532;
                        data.EngineVersion = 30765;
                        break;

                    case Platform.PSVita:
                        data.Version = 7;
                        data.Unknown1 = 6;
                        data.Unknown2 = 0;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 19689438;
                        data.EngineVersion = 0;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Settings.Platform), Settings.Platform, null);
                }

                break;

            case BinarySerializer.UbiArt.Game.GravityFalls:

                switch (Settings.Platform)
                {
                    case Platform.Nintendo3DS:
                        data.Version = 7;
                        data.Unknown1 = 10;
                        data.Unknown2 = 0;
                        data.Unknown3 = false;
                        data.Unknown4 = true;
                        data.Unknown5 = true;
                        data.Unknown6 = 0;
                        data.Unknown7 = 4160251604;
                        data.EngineVersion = 0;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Settings.Platform), Settings.Platform, null);
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(Settings.Game), Settings.Game, null);
        }
    }

    /// <summary>
    /// Gets a value indicating if the file should be compressed
    /// </summary>
    /// <param name="entry">The file entry to check</param>
    /// <returns>True if the file should be compressed, otherwise false</returns>
    public bool ShouldCompress(BundleFile_FileEntry entry)
    {
        return CompressionMode switch
        {
            FileCompressionMode.Never => false,
            FileCompressionMode.Always => true,

            FileCompressionMode.MatchesSetting => !CompressEntireBlock && CompressedFileExtensions.Any(x => x == new FileExtension(entry.Path.FileName, multiple: true)),
            FileCompressionMode.WasCompressed => entry.IsCompressed,

            _ => throw new ArgumentOutOfRangeException(nameof(CompressionMode), CompressionMode, null)
        };
    }

    /// <summary>
    /// Gets a value indicating if the data block should be compressed
    /// </summary>
    /// <param name="data">The .ipk data to check</param>
    /// <returns>True if the data block should be compressed, otherwise false</returns>
    public bool ShouldCompress(BundleBootHeader data)
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