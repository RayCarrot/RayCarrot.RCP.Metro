namespace RayCarrot.RCP.Metro.Imaging;

public class ImageMetadata
{
    public ImageMetadata(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }
    
    public int MipmapsCount { get; init; }
    public string? Encoding { get; init; }

    public IEnumerable<DuoGridItemViewModel> GetInfoItems(ImageFormat? format)
    {
        if (format != null)
            yield return new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Format)),
                text: new GeneratedLocString(() => format.Name));

        yield return new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Size)),
            text: $"{Width}x{Height}");

        if (MipmapsCount != 0)
            yield return new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Mipmaps)),
                text: MipmapsCount.ToString(),
                minUserLevel: UserLevel.Technical);

        if (Encoding != null)
            yield return new DuoGridItemViewModel(
                header: new ResourceLocString("Encoding"), // TODO-LOC
                text: Encoding,
                minUserLevel: UserLevel.Technical);
    }
}