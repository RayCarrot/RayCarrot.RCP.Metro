using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace RayCarrot.RCP.Metro;

public class ImageScalingBehavior : Behavior<Image>
{
    public BitmapScalingMode DownscaleScalingMode { get; set; }
    public BitmapScalingMode UpscaleScalingMode { get; set; }

    private void AssociatedObject_SourceChanged(object sender, EventArgs e)
    {
        UpdateScalingMode();
    }

    private void UpdateScalingMode()
    {
        if (AssociatedObject.Source == null)
            return;

        if (AssociatedObject.Source.Width > AssociatedObject.Width)
            RenderOptions.SetBitmapScalingMode(AssociatedObject, DownscaleScalingMode);
        else if (AssociatedObject.Source.Width < AssociatedObject.Width)
            RenderOptions.SetBitmapScalingMode(AssociatedObject, UpscaleScalingMode);
        else
            RenderOptions.SetBitmapScalingMode(AssociatedObject, BitmapScalingMode.Unspecified);
    }

    protected override void OnAttached()
    {
        DependencyPropertyDescriptor descr = DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image));
        descr.AddValueChanged(AssociatedObject, AssociatedObject_SourceChanged);
        UpdateScalingMode();
    }

    protected override void OnDetaching()
    {
        DependencyPropertyDescriptor descr = DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image));
        descr.RemoveValueChanged(AssociatedObject, AssociatedObject_SourceChanged);
    }
}