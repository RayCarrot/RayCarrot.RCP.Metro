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
}