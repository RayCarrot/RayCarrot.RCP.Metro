using System.IO;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Imaging;

namespace RayCarrot.RCP.Metro;

// TODO: Rewrite this to work better with the new ImageFormat classes, like we've done in the Archive Explorer
public class Utility_Converters_CPAGF_TypeViewModel : Utility_Converters_TypeViewModel
{
    public Utility_Converters_CPAGF_TypeViewModel(LocalizedString name, ObservableCollection<Utility_SerializableTypeModeViewModel> modes) 
        : base(name, modes) { }

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
        OpenSpaceSettings settings = context.GetRequiredSettings<OpenSpaceSettings>();
        CpaGfImageFormat inputFormat = new(settings);
        ImageFormat outputFormat = outputFilePath.FileExtensions.PrimaryFileExtension switch
        {
            ".png" => new PngImageFormat(),
            ".jpeg" => new JpgImageFormat(),
            ".jpg" => new JpgImageFormat(),
            ".bmp" => new BmpImageFormat(),
            _ => throw new Exception($"The specified file format {outputFilePath.FileExtensions} is not supported")
        };

        using FileStream inputStream = File.OpenRead(context.GetAbsoluteFilePath(inputFileName));
        using FileStream outputStream = File.Create(outputFilePath);
        inputFormat.Convert(inputStream, outputStream, outputFormat);
    }

    public override async Task<object?> GetConvertBackStateAsync()
    {
        // Allow the user to select the format
        string[] formats = Enum.GetNames(typeof(GF_Format));
        ItemSelectionDialogResult extResult = await Services.UI.SelectItemAsync(new ItemSelectionDialogViewModel(formats, Resources.Utilities_Converter_SelectConvertBackFormat));

        if (extResult.CanceledByUser)
            return null;

        return formats[extResult.SelectedIndex];
    }

    public override void ConvertBack(Context context, FileSystemPath inputFilePath, string outputFileName, object state)
    {
        GF_Format gfFormat = (GF_Format)Enum.Parse(typeof(GF_Format), state.ToString());

        OpenSpaceSettings settings = context.GetRequiredSettings<OpenSpaceSettings>();
        ImageFormat inputFormat = inputFilePath.FileExtensions.PrimaryFileExtension switch
        {
            ".png" => new PngImageFormat(),
            ".jpeg" => new JpgImageFormat(),
            ".jpg" => new JpgImageFormat(),
            ".bmp" => new BmpImageFormat(),
            _ => throw new Exception($"The specified file format {inputFilePath.FileExtensions} is not supported")
        };
        CpaGfImageFormat outputFormat = new(settings);

        using FileStream inputStream = File.OpenRead(inputFilePath);
        using FileStream outputStream = File.Create(context.GetAbsoluteFilePath(outputFileName));
        RawImageData decodedData = inputFormat.Decode(inputStream);
        outputFormat.Encode(decodedData, outputStream, Services.Data.Archive_GF_GenerateMipmaps, gfFormat);
    }
}