using System;
using RayCarrot.IO;
using RayCarrot.Rayman.OpenSpace;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.Extensions;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting .gf files
    /// </summary>
    public class GFConverterUtilityViewModel : BaseConverterUtilityViewModel<OpenSpaceGameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GFConverterUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2PC, new OpenSpaceGameMode[]
            {
                OpenSpaceGameMode.Rayman2PC,
                OpenSpaceGameMode.Rayman2IOS,
                OpenSpaceGameMode.Rayman2PCDemo1,
                OpenSpaceGameMode.Rayman2PCDemo2,
                OpenSpaceGameMode.RaymanMPC,
                OpenSpaceGameMode.RaymanArenaPC,
                OpenSpaceGameMode.Rayman3PC,
                OpenSpaceGameMode.TonicTroublePC,
                OpenSpaceGameMode.TonicTroubleSEPC,
                OpenSpaceGameMode.DonaldDuckPC,
                OpenSpaceGameMode.PlaymobilHypePC,
                OpenSpaceGameMode.PlaymobilLauraPC,
                OpenSpaceGameMode.PlaymobilAlexPC,
                OpenSpaceGameMode.DinosaurPC,
                OpenSpaceGameMode.LargoWinchPC
            });
        }

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<OpenSpaceGameMode> GameModeSelection { get; }

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
                gf.ImportFromBitmap(settings, new RawBitmapData(bmp), RCFRCP.Data.Archive_GF_GenerateMipmaps);

                // Return the data
                return gf;
            }, new FileFilterItemCollection(ImageHelpers.GetSupportedBitmapExtensions().Select(x => new FileFilterItem($"*{x}",
                x.Substring(1).ToUpper()))).ToString(), new FileExtension(".gf"), Enum.GetNames(typeof(OpenSpaceGFFormat)));
        }

        #endregion
    }
}