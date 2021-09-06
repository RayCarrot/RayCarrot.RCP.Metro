using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting .gf files
    /// </summary>
    public class Utility_Converter_GF_ViewModel : Utility_BaseConverter_ViewModel<GameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Utility_Converter_GF_ViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.Rayman2PC, new GameMode[]
            {
                GameMode.Rayman2PC,
                GameMode.Rayman2IOS,
                GameMode.Rayman2PCDemo1,
                GameMode.Rayman2PCDemo2,
                GameMode.RaymanMPC,
                GameMode.RaymanArenaPC,
                GameMode.Rayman3PC,
                GameMode.TonicTroublePC,
                GameMode.TonicTroubleSEPC,
                GameMode.DonaldDuckPC,
                GameMode.PlaymobilHypePC,
                GameMode.PlaymobilLauraPC,
                GameMode.PlaymobilAlexPC,
                GameMode.DinosaurPC,
                GameMode.LargoWinchPC
            });
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the commonly supported bitmap file extensions
        /// </summary>
        /// <returns>The commonly supported bitmap file extensions</returns>
        private string[] GetSupportedBitmapExtensions()
        {
            return new string[]
            {
                ".png",
                ".jpg",
                ".jpeg",
                ".bmp",
            };
        }

        /// <summary>
        /// Gets the <see cref="ImageFormat"/> from the specified file extension
        /// </summary>
        /// <param name="fileExtension">The file extension</param>
        /// <returns>The image format</returns>
        private ImageFormat GetImageFormat(FileExtension fileExtension)
        {
            // Get the format
            return fileExtension.PrimaryFileExtension switch
            {
                ".png" => ImageFormat.Png,
                ".jpeg" => ImageFormat.Jpeg,
                ".jpg" => ImageFormat.Jpeg,
                ".bmp" => ImageFormat.Bmp,
                _ => throw new Exception($"The specified file format {fileExtension} is not supported")
            };
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Converts from the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertFromAsync()
        {
            var attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();
            var settings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

            await ConvertFromAsync<OpenSpaceGFFile>(settings, (data, filePath) =>
            {
                // Get a bitmap from the image data
                using var bmp = data.GetRawBitmapData().GetBitmap();

                // Save the image
                bmp.Save(filePath, GetImageFormat(filePath.FileExtension));
            }, new FileFilterItem("*.gf", "GF").ToString(), GetSupportedBitmapExtensions(), null);
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            var attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();
            var settings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

            await ConvertToAsync<OpenSpaceGFFile>(settings, (filePath, format) =>
            {
                // Create the GF data
                var gf = new OpenSpaceGFFile
                {
                    // Set the .gf format
                    GFPixelFormat = Enum.Parse(typeof(OpenSpaceGFFormat), format).CastTo<OpenSpaceGFFormat>()
                };

                // Read the image
                using var bmp = new Bitmap(filePath);

                // IDEA: If bmp is not in supported format, then convert it?

                var oldRepeatByte = gf.RepeatByte;

                // Import from the bitmap
                gf.ImportFromBitmap(settings, new RawBitmapData(bmp), RCPServices.Data.Archive_GF_GenerateMipmaps);

                Logger.Debug("The repeat byte has been updated for a .gf file from {0} to {1}", oldRepeatByte, gf.RepeatByte);

                // Return the data
                return gf;
            }, new FileFilterItemCollection(GetSupportedBitmapExtensions().Select(x => new FileFilterItem($"*{x}",
                x.Substring(1).ToUpper()))).ToString(), new FileExtension(".gf"), Enum.GetNames(typeof(OpenSpaceGFFormat)));
        }

        #endregion
    }
}