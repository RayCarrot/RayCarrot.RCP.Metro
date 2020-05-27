using System;
using RayCarrot.IO;
using RayCarrot.Rayman.OpenSpace;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.Common;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting .gf files
    /// </summary>
    public class GFConverterUtilityViewModel : BaseConverterUtilityViewModel<GameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GFConverterUtilityViewModel()
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

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }

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
                bmp.Save(filePath, ImageHelpers.GetImageFormat(filePath.FileExtension));
            }, new FileFilterItem("*.gf", "GF").ToString(), ImageHelpers.GetSupportedBitmapExtensions(), null);
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

                // Import from the bitmap
                gf.ImportFromBitmap(settings, new RawBitmapData(bmp), RCPServices.Data.Archive_GF_GenerateMipmaps);

                // Return the data
                return gf;
            }, new FileFilterItemCollection(ImageHelpers.GetSupportedBitmapExtensions().Select(x => new FileFilterItem($"*{x}",
                x.Substring(1).ToUpper()))).ToString(), new FileExtension(".gf"), Enum.GetNames(typeof(OpenSpaceGFFormat)));
        }

        #endregion
    }
}