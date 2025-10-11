using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public class WebImageCache
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, BitmapImage> _cache = new();

    public ImageSource LoadImageSource(string url, int decodePixelWidth, int decodePixelHeight)
    {
        lock (_cache)
        {
            if (_cache.TryGetValue(url, out BitmapImage imgSource))
            {
                if (imgSource.DecodePixelWidth != decodePixelWidth ||
                    imgSource.DecodePixelHeight != decodePixelHeight)
                {
                    Logger.Warn("Loading cached image source with different decode size than requested");
                }

                return imgSource;
            }

            Uri uri = new(url);

            imgSource = new();
            imgSource.BeginInit();
            imgSource.CacheOption = BitmapCacheOption.OnLoad;
            imgSource.DecodePixelWidth = decodePixelWidth;
            imgSource.DecodePixelHeight = decodePixelHeight;
            imgSource.UriSource = uri;
            imgSource.EndInit();

            if (imgSource.CanFreeze)
                imgSource.Freeze();

            _cache.Add(url, imgSource);

            return imgSource;
        }
    }
}