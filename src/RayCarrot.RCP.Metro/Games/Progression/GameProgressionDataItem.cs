using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A progression data item
/// </summary>
public class GameProgressionDataItem
{
    public GameProgressionDataItem(bool isPrimaryItem, ProgressionIconAsset icon, LocalizedString header, LocalizedString text)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = text;
        Header = header;
    }

    public GameProgressionDataItem(bool isPrimaryItem, ProgressionIconAsset icon, LocalizedString header, int value)
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

    public GameProgressionDataItem(bool isPrimaryItem, ProgressionIconAsset icon, LocalizedString header, int value, int max)
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
    public ProgressionIconAsset Icon { get; }

    public LocalizedString Header { get; }
    public LocalizedString Text { get; }
}