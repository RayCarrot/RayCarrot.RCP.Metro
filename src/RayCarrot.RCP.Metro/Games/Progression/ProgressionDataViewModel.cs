using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A progression data item view model
/// </summary>
public class ProgressionDataViewModel : BaseViewModel
{
    public ProgressionDataViewModel(bool isPrimaryItem, ProgressionIcon icon, LocalizedString header, LocalizedString text)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = text;
        Header = header;
    }

    public ProgressionDataViewModel(bool isPrimaryItem, ProgressionIcon icon, LocalizedString header, int value)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = value.ToString("n", new NumberFormatInfo()
        {
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 0
        });
        Header = header;
    }

    public ProgressionDataViewModel(bool isPrimaryItem, ProgressionIcon icon, LocalizedString header, int value, int max)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = $"{value} / {max}";
        Header = header;
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

    public LocalizedString Header { get; }
    public LocalizedString Text { get; }
}