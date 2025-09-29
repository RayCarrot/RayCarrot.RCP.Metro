using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RayCarrot.RCP.Metro;

// Since the DownloadCompleted event is unreliable we have this custom Image class with a property for if it's loaded
public class ImageEx : Image
{
    public ImageEx()
    {
        LayoutUpdated += ImageEx_OnLayoutUpdated;
    }

    public bool HasLoaded
    {
        get => (bool)GetValue(HasLoadedProperty);
        set => SetValue(HasLoadedProperty, value);
    }

    public static readonly DependencyProperty HasLoadedProperty = DependencyProperty.Register(
        name: nameof(HasLoaded),
        propertyType: typeof(bool),
        ownerType: typeof(ImageEx));

    private void ImageEx_OnLayoutUpdated(object sender, EventArgs e)
    {
        try
        {
            if (Source is BitmapImage { IsDownloading: false })
            {
                if (!HasLoaded)
                    HasLoaded = true;
            }
            else
            {
                if (HasLoaded)
                    HasLoaded = false;
            }
        }
        catch
        {
            // Do nothing
        }
    }
}