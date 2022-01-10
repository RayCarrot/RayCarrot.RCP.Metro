using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using BinarySerializer;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class Utility_Converters_OpenSpaceGF_TypeViewModel : Utility_Converters_TypeViewModel
{
    public Utility_Converters_OpenSpaceGF_TypeViewModel(LocalizedString name, ObservableCollection<Utility_SerializableTypeModeViewModel> modes) : base(name, modes) { }

    public override FileExtension SourceFileExtension => new FileExtension(".gf");

    public override string[] ConvertFormats => new[]
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
    };

    public override void Convert(Context context, string inputFileName, FileSystemPath outputFilePath)
    {
        GF? gf = ReadFile<GF>(context, context.GetSettings<OpenSpaceSettings>().GetEndian, inputFileName);

        if (gf is null)
            return;

        // Get a bitmap from the image data
        using Bitmap bmp = gf.GetRawBitmapData().GetBitmap();

        // Save the image
        bmp.Save(outputFilePath, outputFilePath.FileExtension.PrimaryFileExtension switch
        {
            ".png" => ImageFormat.Png,
            ".jpeg" => ImageFormat.Jpeg,
            ".jpg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            _ => throw new Exception($"The specified file format {outputFilePath.FileExtension} is not supported")
        });
    }

    public override async Task<object?> GetConvertBackStateAsync()
    {
        // Allow the user to select the format
        FileExtensionSelectionDialogResult extResult = await Services.UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(Enum.GetNames(typeof(GF_Format)), Resources.Utilities_Converter_SelectConvertBackFormat));

        if (extResult.CanceledByUser)
            return null;

        return extResult.SelectedFileFormat;
    }

    public override void ConvertBack(Context context, FileSystemPath inputFilePath, string outputFileName, object state)
    {
        // Create the GF data
        GF gf = new()
        {
            // Set the .gf format
            PixelFormat = (GF_Format)Enum.Parse(typeof(GF_Format), state.ToString()),
        };

        // Read the image
        using Bitmap bmp = new(inputFilePath);

        // IDEA: If bmp is not in supported format, then convert it?

        OpenSpaceSettings settings = context.GetSettings<OpenSpaceSettings>();

        // Import from the bitmap
        gf.ImportFromBitmap(settings, new RawBitmapData(bmp), Services.Data.Archive_GF_GenerateMipmaps);

        // Write the data to the output file
        WriteFile(context, settings.GetEndian, outputFileName, gf);
    }
}