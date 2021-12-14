using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A progression data item view model
/// </summary>
public class ProgressionDataViewModel : BaseViewModel
{
    public ProgressionDataViewModel(bool isPrimaryItem, ProgressionIcon icon, LocalizedString text, LocalizedString? description = null)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = text;
        Description = description;
    }

    public ProgressionDataViewModel(bool isPrimaryItem, ProgressionIcon icon, int value, LocalizedString? description = null)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = new ConstLocString(value.ToString("n", new NumberFormatInfo()
        {
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 0
        }));
        Description = description;
    }

    public ProgressionDataViewModel(bool isPrimaryItem, ProgressionIcon icon, int value, int max, LocalizedString? description = null)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = new ConstLocString($"{value} / {max}");
        Description = description;
    }

    public bool IsPrimaryItem { get; }

    /// <summary>
    /// The icon
    /// </summary>
    public ProgressionIcon Icon { get; }

    /// <summary>
    /// The icon as an image source
    /// </summary>
    public ImageSource? IconImageSource => new ImageSourceConverter().ConvertFrom($"{AppViewModel.WPFApplicationBasePath}Img/ProgressionIcons/{Icon}.png") as ImageSource;

    public LocalizedString Text { get; }
    public LocalizedString? Description { get; }
}