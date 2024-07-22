namespace RayCarrot.RCP.Metro.Image;

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