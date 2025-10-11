using System.Windows.Media;

namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public class WebImageViewModel : BaseViewModel
{
    public WebImageViewModel(WebImageCache cache)
    {
        _cache = cache;
    }

    private readonly WebImageCache _cache;

    public string? Url { get; set; }
    public int DecodePixelWidth { get; set; }
    public int DecodePixelHeight { get; set; }

    public ImageSource? ImageSource { get; set; }

    public void Load()
    {
        // Do not load if already loaded
        if (ImageSource != null)
            return;

        // Make sure we have a URL
        if (Url == null)
            return;

        ImageSource = _cache.LoadImageSource(Url, DecodePixelWidth, DecodePixelHeight);
    }
}