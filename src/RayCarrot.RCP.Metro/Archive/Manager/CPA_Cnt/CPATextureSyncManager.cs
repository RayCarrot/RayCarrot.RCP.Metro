﻿using System.Diagnostics;
using System.IO;
using System.Text;
using BinarySerializer;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro.Archive.CPA;

public class CPATextureSyncManager
{
    #region Constructor

    public CPATextureSyncManager(
        GameInstallation gameInstallation,
        OpenSpaceSettings gameSettings, 
        CPATextureSyncDataItem[] textureSyncItems)
    {
        GameInstallation = gameInstallation;
        GameSettings = gameSettings;
        TextureSyncItems = textureSyncItems;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public OpenSpaceSettings GameSettings { get; }
    public CPATextureSyncDataItem[] TextureSyncItems { get; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Edits all found texture info objects in CPA data files (.sna, .lvl etc.) to have their resolution match the .gf textures
    /// </summary>
    /// <param name="files">The files to edit</param>
    /// <param name="cntFiles">The .cnt file paths</param>
    /// <param name="progressCallback">An optional progress callback</param>
    /// <returns>The result</returns>
    private TextureInfoEditResult EditTextureInfo(
        IList<FileSystemPath> files,
        IEnumerable<FileSystemPath> cntFiles,
        Action<Progress>? progressCallback = null)
    {
        // Start by invoking an empty progress callback since it takes a bit before we
        // start tracking the actual progress
        progressCallback?.Invoke(Progress.Empty);

        // The offset for the size from the name
        int sizeOffset = GameSettings.MajorEngineVersion switch
        {
            MajorEngineVersion.TonicTrouble => 52,
            MajorEngineVersion.Rayman2 => 42,
            MajorEngineVersion.Rayman3 => 46,

            // Other versions are not yet supported...
            _ => throw new ArgumentOutOfRangeException(nameof(GameSettings.EngineVersion), GameSettings.EngineVersion, null)
        };

        // NOTE: Although TT uses 32-bit integers for the sizes we use ushorts anyway since they never exceed the ushort max size
        // Indicates if sizes are 32-bit
        bool is32Bit = GameSettings.MajorEngineVersion == MajorEngineVersion.TonicTrouble;

        // Create a list of .gf files to read into
        List<GFFileSizeData> gfFiles = new List<GFFileSizeData>();

        // Read every CNT file
        foreach (FileSystemPath cntFile in cntFiles)
        {
            // Create a context
            using RCPContext context = new(cntFile.Parent);
            context.AddFile(new LinearFile(context, cntFile.Name, GameSettings.GetEndian));

            // Read the CNT data
            CNT cntData = FileFactory.Read<CNT>(context, cntFile.Name);

            // Read the size from every .gf file
            gfFiles.AddRange(cntData.Files.
                Where(x => x.FileName.EndsWith(".gf", StringComparison.InvariantCultureIgnoreCase)).
                Select(x =>
                {
                    int width = 0;
                    int height = 0;
                    uint mipmaps = 0;

                    // Read the GF header
                    cntData.ReadFile(x, s =>
                    {
                        // Skip the format
                        if (GameSettings.EngineVersion != EngineVersion.TonicTroubleSpecialEdition)
                            s.Serialize<uint>(default, "Format");

                        // Read the size
                        width = s.Serialize<int>(default, name: "Width");
                        height = s.Serialize<int>(default, name: "Height");

                        if (GameSettings.MajorEngineVersion == MajorEngineVersion.Rayman3)
                        {
                            // Skip the channel count...
                            s.Serialize<byte>(default, "Channels");

                            // Read mipmap count
                            mipmaps = s.Serialize<byte>(default, "MipmapsCount");
                        }
                    }, logIfNotFullyRead: false);

                    // Return the GF data
                    return new GFFileSizeData(x.GetFullPath(cntData.Directories), (ushort)height, (ushort)width, mipmaps);
                }));
        }

        // Make sure we have any GF files
        if (!gfFiles.Any())
            return new TextureInfoEditResult(0, 0);

        // The size of the largest file name
        int largestNameSize = gfFiles.OrderByDescending(x => x.FullPathWithoutExtension.Length).First().FullPathWithoutExtension.Length;

        // Keep track of the count
        int total = 0;
        int edited = 0;

        // Create an encoder
        IStreamEncoder? encoder = GameSettings.EngineVersion switch
        {
            EngineVersion.TonicTrouble => new TTSNADataEncoder(),
            EngineVersion.Rayman2 or EngineVersion.DonaldDuckQuackAttack => new R2SNADataEncoder(),
            _ => null
        };

        // Most games just use .tga, but Tonic Trouble uses the other ones too
        byte[][] fileExtensions =
        {
            ".tga"u8.ToArray(),
            ".TGA"u8.ToArray(),
            ".bmp"u8.ToArray(),
            ".BMP"u8.ToArray(),
        };

        // Enumerate each file
        for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
        {
            FileSystemPath file = files[fileIndex];

            progressCallback?.Invoke(new Progress(fileIndex, files.Count));

            // Keep track of the number of found textures
            int foundCount = 0;

            // Read the file data
            byte[] data = File.ReadAllBytes(file);

            // Decode if we have an encoder
            if (encoder != null)
                data = encoder.DecodeBuffer(data);

            // Enumerate each byte
            for (int i = sizeOffset + largestNameSize; i < data.Length - 4; i++)
            {
                // Make sure the position contains a file extension used at the end of a file path
                if (data[i] != 0x2E)
                    continue;

                // Make sure the position contains a supported file extension
                if (fileExtensions.All(x => data[i + 1] != x[1] || data[i + 2] != x[2] || data[i + 3] != x[3]))
                    continue;

                total++;

                // NOTE: Windows 1252 is used rather than UTF-8 here
                // Get the longest possible name
                string longestName = Encoding.GetEncoding(1252).GetString(data, i - largestNameSize, largestNameSize);

                if (GameSettings.MajorEngineVersion == MajorEngineVersion.TonicTrouble)
                    longestName = longestName.Replace('/', '\\');

                // Find the matching file
                GFFileSizeData? gf = gfFiles.
                    Where(x => longestName.EndsWith(x.FullPathWithoutExtension, StringComparison.InvariantCultureIgnoreCase)).
                    OrderByDescending(x => x.FullPathWithoutExtension.Length).
                    FirstOrDefault();

                // Ignore if not found
                if (gf == null)
                {
                    Logger.Warn("A matching texture was not found for {0}", longestName.Trim('\0'));
                    continue;
                }

                // Get the length of the path
                int pathLength = gf.FullPathWithoutExtension.Length;

                // Get the current sizes from the .sna file
                uint snaHeight = is32Bit
                    ? BitConverter.ToUInt32(data, i - pathLength - sizeOffset)
                    : BitConverter.ToUInt16(data, i - pathLength - sizeOffset);
                uint snaWidth = is32Bit
                    ? BitConverter.ToUInt32(data, i - pathLength - sizeOffset + 4)
                    : BitConverter.ToUInt16(data, i - pathLength - sizeOffset + 2);

                // Get the size from the .gf file
                ushort gfHeight = gf.Height;
                ushort gfWidth = gf.Width;

                // A hacky solution to an issue in Tonic Trouble where the texture names appear in some different structs which we
                // don't want to accidentally modify. This will filter those out (at least in the version I tested this on).
                if (!validateSize(snaWidth) || !validateSize(snaHeight))
                {
                    total--;
                    continue;
                }

                // Also check the gf file size so it's not invalid. This could happen if the user accidentally corrupts the file.
                // If that happens we don't want to write the corrupt size into the sna file since then syncing won't work again.
                if (!validateSize(gfWidth) || !validateSize(gfHeight))
                {
                    total--;
                    continue;
                }

                bool validateSize(uint size) => size is not (0 or >= 32767);

                if (GameSettings.MajorEngineVersion == MajorEngineVersion.Rayman2)
                {
                    uint flags_TextureCaps = BitConverter.ToUInt32(data, i - pathLength - sizeOffset - 8);
                    byte flags_CyclingMode = data[i - pathLength - sizeOffset + 41];

                    if ((flags_CyclingMode & 0x4) != 0)
                        gfWidth *= 2; // Mirror X

                    if ((flags_CyclingMode & 0x8) != 0)
                        gfHeight *= 2; // Mirror Y

                    if ((flags_TextureCaps & 0x400) != 0)
                        gfHeight /= 2;
                }

                // Correct the aspect ratio
                if (snaWidth < snaHeight)
                {
                    double ratio = snaHeight / (double)snaWidth;
                    gfHeight = (ushort)(gfWidth * ratio);
                }
                else
                {
                    double ratio = snaWidth / (double)snaHeight;
                    gfWidth = (ushort)(gfHeight * ratio);
                }

                // Get the bytes for the sizes
                byte[] heightBytes = is32Bit ? BitConverter.GetBytes((uint)gfHeight) : BitConverter.GetBytes(gfHeight);
                byte[] widthBytes = is32Bit ? BitConverter.GetBytes((uint)gfWidth) : BitConverter.GetBytes(gfWidth);

                int byteIndex = 0;

                // Set the new sizes
                foreach (var b in heightBytes.Concat(widthBytes))
                {
                    data[i - pathLength - sizeOffset + byteIndex] = b;
                    byteIndex++;
                }

                // Set mipmaps if available
                if (GameSettings.MajorEngineVersion == MajorEngineVersion.Rayman3)
                {
                    // Get the mipmap bytes
                    byte[] mipmapBytes = BitConverter.GetBytes(gf.MipmapCount);

                    // The base offset
                    const int mipmapOffset = 22;

                    // Set the mipmap count
                    data[i - pathLength - mipmapOffset + 0] = mipmapBytes[0];
                    data[i - pathLength - mipmapOffset + 1] = mipmapBytes[1];
                    data[i - pathLength - mipmapOffset + 2] = mipmapBytes[2];
                    data[i - pathLength - mipmapOffset + 3] = mipmapBytes[3];
                }

                foundCount++;
                edited++;
            }

            Logger.Info("{0} texture infos modified for {1}", foundCount, file.Name);

            // Encode if we have an encoder
            if (encoder != null)
                data = encoder.EncodeBuffer(data);

            // Write the new data
            File.WriteAllBytes(file, data);
        }

        progressCallback?.Invoke(Progress.Complete);

        // Return the result
        return new TextureInfoEditResult(total, edited);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Synchronizes the texture info for the selected game data directory
    /// </summary>
    /// <param name="specificCntFiles">The .cnt file paths, or null if they should be automatically found</param>
    /// <param name="progressCallback">An optional progress callback</param>
    /// <returns>The task</returns>
    public async Task SyncTextureInfoAsync(
        IReadOnlyList<FileSystemPath>? specificCntFiles = null,
        Action<Progress>? progressCallback = null)
    {
        try
        {
            TextureInfoEditResult syncResult = await Task.Run(() =>
            {
                // Get the game install directory
                FileSystemPath installDir = GameInstallation.InstallLocation.Directory;

                // Get the file extension for the level data files
                string fileExt = GameSettings.MajorEngineVersion switch
                {
                    MajorEngineVersion.TonicTrouble => ".sna",
                    MajorEngineVersion.Rayman2 => ".sna",
                    MajorEngineVersion.Rayman3 => ".lvl",
                    _ => throw new ArgumentOutOfRangeException(nameof(GameSettings.EngineVersion), GameSettings.EngineVersion, null)
                };

                int total = 0;
                int edited = 0;

                for (int i = 0; i < TextureSyncItems.Length; i++)
                {
                    CPATextureSyncDataItem syncItem = TextureSyncItems[i];
                    // Get the level data files
                    List<FileSystemPath> dataFiles =
                        Directory.GetFiles(installDir + syncItem.Name, $"*{fileExt}", SearchOption.AllDirectories)
                            .Select(x => new FileSystemPath(x)).ToList();

                    // Get all of the cnt files which exist for this item. If no files are provided we use all of the available ones.
                    List<FileSystemPath> cntFiles = syncItem.Archives.Select(cnt => installDir + syncItem.Name + cnt)
                        .Where(cntPath => cntPath.FileExists).ToList();

                    // If files are provided we only use the ones which are available for this item
                    if (specificCntFiles != null)
                        cntFiles.RemoveAll(x => !specificCntFiles.Contains(x));

                    // Sync the texture info
                    int syncItemIndex = i;
                    TextureInfoEditResult result = EditTextureInfo(dataFiles, cntFiles,
                        progressCallback == null ? null : x =>
                        {
                            progressCallback(new Progress(syncItemIndex + x.Percentage, TextureSyncItems.Length));
                        });

                    total += result.TotalTextures;
                    edited += result.EditedTextures;
                }

                return new TextureInfoEditResult(total, edited);
            });

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Utilities_SyncTextureInfo_Success, syncResult.EditedTextures, syncResult.TotalTextures));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Syncing texture info");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_SyncTextureInfo_Error);
        }
    }

    #endregion

    #region Data Types

    /// <summary>
    /// Data for a .gf file, containing its size and path
    /// </summary>
    [DebuggerDisplay("{FullPathWithoutExtension} - {Width}x{Height}")]
    private class GFFileSizeData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fullPath">The full .gf file path</param>
        /// <param name="height">The image height</param>
        /// <param name="width">The image width</param>
        /// <param name="mipmapCount">The available mipmaps</param>
        public GFFileSizeData(string fullPath, ushort height, ushort width, uint mipmapCount)
        {
            FullPathWithoutExtension = fullPath.Remove(fullPath.Length - 3);
            Height = height;
            Width = width;
            MipmapCount = mipmapCount;
        }

        /// <summary>
        /// The full .gf file path without the file extension
        /// </summary>
        public string FullPathWithoutExtension { get; }

        /// <summary>
        /// The image height
        /// </summary>
        public ushort Height { get; }

        /// <summary>
        /// The image width
        /// </summary>
        public ushort Width { get; }

        /// <summary>
        /// The available mipmaps
        /// </summary>
        public uint MipmapCount { get; }
    }

    /// <summary>
    /// The result for editing the texture info
    /// </summary>
    /// <param name="TotalTextures">The total found textures infos across all maps</param>
    /// <param name="EditedTextures">The number of edited texture infos across all maps</param>
    private record TextureInfoEditResult(int TotalTextures, int EditedTextures);

    #endregion
}