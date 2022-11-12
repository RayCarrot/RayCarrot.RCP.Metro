using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

// TODO: Remove IconImageSource and use a converter instead

/// <summary>
/// A progression data item
/// </summary>
public class GameProgressionDataItem
{
    public GameProgressionDataItem(bool isPrimaryItem, ProgressionIcon icon, LocalizedString header, LocalizedString text)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = text;
        Header = header;
    }

    public GameProgressionDataItem(bool isPrimaryItem, ProgressionIcon icon, LocalizedString header, int value)
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

    public GameProgressionDataItem(bool isPrimaryItem, ProgressionIcon icon, LocalizedString header, int value, int max)
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