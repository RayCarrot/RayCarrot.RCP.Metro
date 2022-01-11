using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Base view model for synchronizing texture info
/// </summary>
public abstract class Utility_BaseSyncTextureInfoViewModel : BaseRCPViewModel
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Protected Methods

    /// <summary>
    /// Gets the file extension for the level data files
    /// </summary>
    /// <param name="gameSettings">The settings</param>
    /// <returns>The file extension</returns>
    protected string GetLevelFileExtension(OpenSpaceSettings gameSettings)
    {
        return gameSettings.MajorEngineVersion switch
        {
            MajorEngineVersion.TonicTrouble => ".sna",
            MajorEngineVersion.Rayman2 => ".sna",
            MajorEngineVersion.Rayman3 => ".lvl",
            _ => throw new ArgumentOutOfRangeException(nameof(gameSettings.EngineVersion), gameSettings.EngineVersion, null)
        };
    }

    /// <summary>
    /// Gets the file names for the .cnt files. If a game has more than one version, such as 16-bit and 32-bit textures, the highest quality ones are chosen.
    /// </summary>
    /// <param name="gameSettings">The settings</param>
    /// <returns>The file names</returns>
    protected string[] GetCntFileNames(OpenSpaceSettings gameSettings)
    {
        return gameSettings.EngineVersion switch
        {
            EngineVersion.TonicTroubleSpecialEdition => new string[]
            {
                "TEXTURES.CNT",
                "VIGNETTE.CNT",
            },
            EngineVersion.TonicTrouble => new string[]
            {
                "Textures.cnt",
                "Vignette.cnt",
            },
            EngineVersion.Rayman2 => new string[]
            {
                "Textures.cnt",
                "Vignette.cnt",
            },
            EngineVersion.RaymanM => new string[]
            {
                "tex32.cnt",
                "vignette.cnt",
            },
            EngineVersion.RaymanArena => new string[]
            {
                "tex32.cnt",
                "vignette.cnt",
            },
            EngineVersion.Rayman3 => new string[]
            {
                "tex32_1.cnt",
                "tex32_2.cnt",
                "vignette.cnt",
            },
            _ => throw new ArgumentOutOfRangeException(nameof(gameSettings.EngineVersion), gameSettings.EngineVersion, null)
        };
    }

    /// <summary>
    /// Edits all found texture info objects in an OpenSpace data files (.sna, .lvl etc.) to have their resolution match the .gf textures
    /// </summary>
    /// <param name="gameSettings">The OpenSpace game settings to use</param>
    /// <param name="files">The files to edit</param>
    /// <param name="cntFiles">The .cnt file paths</param>
    /// <returns>The result</returns>
    protected TextureInfoEditResult EditTextureInfo(OpenSpaceSettings gameSettings, IEnumerable<FileSystemPath> files, IEnumerable<FileSystemPath> cntFiles)
    {
        // The offset for the size from the name
        int sizeOffset = gameSettings.MajorEngineVersion switch
        {
            MajorEngineVersion.TonicTrouble => 52,
            MajorEngineVersion.Rayman2 => 42,
            MajorEngineVersion.Rayman3 => 46,

            // Other versions are not yet supported...
            _ => throw new ArgumentOutOfRangeException(nameof(gameSettings.EngineVersion), gameSettings.EngineVersion, null)
        };

        // NOTE: Although TT uses 32-bit integers for the sizes we use ushorts anyway since they never exceed the ushort max size
        // Indicates if sizes are 32-bit
        bool is32Bit = gameSettings.MajorEngineVersion == MajorEngineVersion.TonicTrouble;

        // Create a list of .gf files to read into
        List<GFFileSizeData> gfFiles = new List<GFFileSizeData>();

        // Read every CNT file
        foreach (FileSystemPath cntFile in cntFiles)
        {
            // Create a context
            using RCPContext context = new(cntFile.Parent);
            context.AddFile(new LinearFile(context, cntFile.Name, gameSettings.GetEndian));

            // Read the CNT data
            CNT cntData = FileFactory.Read<CNT>(context, cntFile.Name);

            // Read the size from every .gf file
            gfFiles.AddRange(cntData.Files.Select(x =>
            {
                int width = 0;
                int height = 0;
                uint mipmaps = 0;

                // Read the GF header
                cntData.ReadFile(x, s =>
                {
                    // Skip the format
                    if (gameSettings.EngineVersion != EngineVersion.TonicTroubleSpecialEdition)
                        s.Serialize<uint>(default, "Format");

                    // Read the size
                    width = s.Serialize<int>(default, name: "Width");
                    height = s.Serialize<int>(default, name: "Height");

                    if (gameSettings.MajorEngineVersion == MajorEngineVersion.Rayman3)
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

        // Create a encoder
        IStreamEncoder? encoder = gameSettings.EngineVersion switch
        {
            EngineVersion.TonicTrouble => new TTSNADataEncoder(),
            EngineVersion.Rayman2 => new R2SNADataEncoder(),
            _ => null
        };

        // Enumerate each file
        foreach (FileSystemPath file in files)
        {
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
                // TODO: Tonic Trouble seems to also have some file extensions as .bmp - do these need to be synced as well?
                // Make sure the position contains the .tga file extension used at the end of a file path
                if (data[i] != 0x2E || data[i + 1] != 0x74 || data[i + 2] != 0x67 || data[i + 3] != 0x61)
                    continue;

                total++;

                // NOTE: Windows 1252 is used rather than UTF-8 here
                // Get the longest possible name
                string longestName = Encoding.GetEncoding(1252).GetString(data, i - largestNameSize, largestNameSize);

                if (gameSettings.MajorEngineVersion == MajorEngineVersion.TonicTrouble)
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
                uint snaHeight = is32Bit ? BitConverter.ToUInt32(data, i - pathLength - sizeOffset) : BitConverter.ToUInt16(data, i - pathLength - sizeOffset);
                uint snaWidth = is32Bit ? BitConverter.ToUInt32(data, i - pathLength - sizeOffset + 4) : BitConverter.ToUInt16(data, i - pathLength - sizeOffset + 2);

                // Get the size from the .gf file
                ushort gfHeight = gf.Height;
                ushort gfWidth = gf.Width;

                if (gameSettings.MajorEngineVersion == MajorEngineVersion.Rayman2)
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
                if (gameSettings.MajorEngineVersion == MajorEngineVersion.Rayman3)
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

        // Return the result
        return new TextureInfoEditResult(total, edited);
    }

    #endregion

    #region Protected Classes

    /// <summary>
    /// Data for a .gf file, containing its size and path
    /// </summary>
    [DebuggerDisplay("{FullPathWithoutExtension} - {Width}x{Height}")]
    protected class GFFileSizeData
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
    protected class TextureInfoEditResult
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="totalTextures">The total found textures infos across all maps</param>
        /// <param name="editedTextures">The number of edited texture infos across all maps</param>
        public TextureInfoEditResult(int totalTextures, int editedTextures)
        {
            TotalTextures = totalTextures;
            EditedTextures = editedTextures;
        }

        /// <summary>
        /// The total found textures infos across all maps
        /// </summary>
        public int TotalTextures { get; }

        /// <summary>
        /// The number of edited texture infos across all maps
        /// </summary>
        public int EditedTextures { get; }
    }

    #endregion
}