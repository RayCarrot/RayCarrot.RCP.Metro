using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for synchronizing texture info
    /// </summary>
    public class SyncTextureInfoUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SyncTextureInfoUtilityViewModel()
        {
            // Create commands
            CorrectTextureInfoCommand = new AsyncRelayCommand(SyncTextureInfoAsync);

            // Set up selection
            GameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2PC, new OpenSpaceGameMode[]
            {
                OpenSpaceGameMode.Rayman2PC,
                OpenSpaceGameMode.RaymanMPC,
                OpenSpaceGameMode.RaymanArenaPC,
                OpenSpaceGameMode.Rayman3PC,
            });
        }

        #endregion

        #region Protected Methods

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
            var sizeOffset = gameSettings.EngineVersion switch
            {
                OpenSpaceEngineVersion.Rayman2 => 42,

                OpenSpaceEngineVersion.Rayman3 => 46,

                // Other versions are not yet supported...
                _ => throw new ArgumentOutOfRangeException(nameof(gameSettings.EngineVersion), gameSettings.EngineVersion, null)
            };

            // Create a list of .gf files to read into
            List<GFFileSizeData> gfFiles = new List<GFFileSizeData>();

            // Read every .cnt file
            foreach (var cntFile in cntFiles)
            {
                // Open the file
                using var cntFileStream = File.OpenRead(cntFile);

                // Read the .cnt data
                var cntData = new OpenSpaceCntSerializer(gameSettings).Deserialize(cntFileStream);

                // Read the size from every .gf file
                gfFiles.AddRange(cntData.Files.Select(x =>
                {
                    using var gfMemoryStream = new MemoryStream(x.GetFileBytes(cntFileStream));

                    // Get a reader
                    using var reader = new StandardBinaryReader(gfMemoryStream, ByteOrder.LittleEndian);

                    // Set the position to where the .gf file is, skipping the format value
                    gfMemoryStream.Position = 4;
                    
                    // Read the size
                    var width = reader.ReadInt32();
                    var height = reader.ReadInt32();

                    uint mipmaps = 0;

                    if (gameSettings.EngineVersion == OpenSpaceEngineVersion.Rayman3)
                    {
                        // Skip the channel count...
                        reader.ReadByte();

                        // Read mipmap count
                        mipmaps = reader.ReadByte();
                    }

                    // Get the .gf data
                    return new GFFileSizeData(x.GetFullPath(cntData.Directories), (ushort)height, (ushort)width, mipmaps);
                }));
            }
            
            // Make sure we have any .gf files
            if (!gfFiles.Any())
                return new TextureInfoEditResult(0, 0);

            // The size of the largest file name
            var largestNameSize = gfFiles.OrderByDescending(x => x.FullPathWithoutExtension.Length).First().FullPathWithoutExtension.Length;

            // Keep track of the count
            int total = 0;
            int edited = 0;

            // Create a Rayman 2 encoder
            var r2Encoder = new Rayman2SNADataEncoder();

            // Enumerate each file
            foreach (var file in files)
            {
                // Keep track of the number of found textures
                int foundCount = 0;

                // Read the file data
                var data = File.ReadAllBytes(file);

                // Decode if the game is Rayman 2
                if (gameSettings.Game == OpenSpaceGame.Rayman2)
                    data = r2Encoder.Decode(data);

                // Enumerate each byte
                for (int i = sizeOffset + largestNameSize; i < data.Length - 4; i++)
                {
                    // Make sure the position contains the .tga file extension used at the end of a file path
                    if (data[i] != 0x2E || data[i + 1] != 0x74 || data[i + 2] != 0x67 || data[i + 3] != 0x61)
                        continue;

                    total++;

                    // NOTE: Windows 1252 is used rather than UTF-8 here
                    // Get the longest possible name
                    var longestName = Encoding.GetEncoding(1252).GetString(data, i - largestNameSize, largestNameSize);

                    // Find the matching file
                    var gf = gfFiles.Find(x => longestName.EndsWith(x.FullPathWithoutExtension, StringComparison.InvariantCultureIgnoreCase));

                    // Ignore if not found
                    if (gf == null)
                    {
                        RCFCore.Logger?.LogWarningSource($"A matching texture was not found for {longestName.Trim('\0')}");
                        continue;
                    }

                    // Get the length of the path
                    var pathLength = gf.FullPathWithoutExtension.Length;

                    // Get the current sizes from the .sna file
                    var snaHeight = BitConverter.ToUInt16(data, i - pathLength - sizeOffset);
                    var snaWidth = BitConverter.ToUInt16(data, i - pathLength - sizeOffset + 2);

                    // Get the size from the .gf file
                    var gfHeight = gf.Height;
                    var gfWidth = gf.Width;

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
                    var heightBytes = BitConverter.GetBytes(gfHeight);
                    var widthBytes = BitConverter.GetBytes(gfWidth);

                    // NOTE: I'm not sure we need to set these sizes too... The game seems to work without them as well. Confirm?
                    data[i - pathLength - sizeOffset - 4] = heightBytes[0];
                    data[i - pathLength - sizeOffset - 3] = heightBytes[1];
                    data[i - pathLength - sizeOffset - 2] = widthBytes[0];
                    data[i - pathLength - sizeOffset - 1] = widthBytes[1];

                    // Set the new sizes
                    data[i - pathLength - sizeOffset] = heightBytes[0];
                    data[i - pathLength - sizeOffset + 1] = heightBytes[1];
                    data[i - pathLength - sizeOffset + 2] = widthBytes[0];
                    data[i - pathLength - sizeOffset + 3] = widthBytes[1];

                    // Set mipmaps if available
                    if (gameSettings.EngineVersion == OpenSpaceEngineVersion.Rayman3)
                    {
                        // Get the mipmap bytes
                        var mipmapBytes = BitConverter.GetBytes(gf.MipmapCount);

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

                RCFCore.Logger?.LogInformationSource($"{foundCount} texture infos modified for {file.Name}");

                // Encode if the game is Rayman 2
                if (gameSettings.Game == OpenSpaceGame.Rayman2)
                    data = r2Encoder.Encode(data);

                // Write the new data
                File.WriteAllBytes(file, data);
            }

            // Return the result
            return new TextureInfoEditResult(total, edited);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public EnumSelectionViewModel<OpenSpaceGameMode> GameModeSelection { get; }

        /// <summary>
        /// Indicates if the utility is loading
        /// </summary>
        public bool IsLoading { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Synchronizes the texture info for the selected game data directory
        /// </summary>
        /// <returns>The task</returns>
        public async Task SyncTextureInfoAsync()
        {
            var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.Utilities_SyncTextureInfo_SelectDirHeader,
                DefaultDirectory = GameModeSelection.SelectedValue.GetGame()?.GetInstallDir(false) ?? FileSystemPath.EmptyPath
            });

            if (result.CanceledByUser)
                return;

            try
            {
                IsLoading = true;

                var syncResult = await Task.Run(() =>
                {
                    var gameSettings = GameModeSelection.SelectedValue.GetSettings();

                    // Get the file extension for the level data files
                    var fileExt = gameSettings.Game switch
                    {
                        OpenSpaceGame.Rayman2 => ".sna",

                        OpenSpaceGame.RaymanM => ".lvl",
                        OpenSpaceGame.RaymanArena => ".lvl",
                        OpenSpaceGame.Rayman3 => ".lvl",

                        _ => throw new ArgumentOutOfRangeException(nameof(gameSettings.Game), gameSettings.Game, null)
                    };

                    // Get the level data files
                    var dataFiles = Directory.GetFiles(result.SelectedDirectory, $"*{fileExt}", SearchOption.AllDirectories).Select(x => new FileSystemPath(x));

                    // Get the .cnt files
                    var cntFiles = Directory.GetFiles(result.SelectedDirectory, "*.cnt", SearchOption.TopDirectoryOnly).Select(x => new FileSystemPath(x));

                    // Sync the texture info
                    return EditTextureInfo(gameSettings, dataFiles, cntFiles);
                });

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Utilities_SyncTextureInfo_Success, syncResult.EditedTextures, syncResult.TotalTextures));
            }
            catch (Exception ex)
            {
                ex.HandleError("Syncing texture info");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_SyncTextureInfo_Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Commands

        public ICommand CorrectTextureInfoCommand { get; }

        #endregion

        #region Classes

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
}