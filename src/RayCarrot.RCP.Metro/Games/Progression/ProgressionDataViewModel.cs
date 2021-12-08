using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A progression data item view model
/// </summary>
public class ProgressionDataViewModel : BaseViewModel
{
    public ProgressionDataViewModel(bool isPrimaryItem, GameProgression_Icon icon, LocalizedString text, LocalizedString? description = null)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = text;
        Description = description;
    }

    public ProgressionDataViewModel(bool isPrimaryItem, GameProgression_Icon icon, int value)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = new ConstLocString(value.ToString("n", new NumberFormatInfo()
        {
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 0
        }));
    }

    public ProgressionDataViewModel(bool isPrimaryItem, GameProgression_Icon icon, int value, int max)
    {
        IsPrimaryItem = isPrimaryItem;
        Icon = icon;
        Text = new ConstLocString($"{value} / {max}");
    }

    public bool IsPrimaryItem { get; }

    /// <summary>
    /// The icon
    /// </summary>
    public GameProgression_Icon Icon { get; }

    /// <summary>
    /// The icon as an image source
    /// </summary>
    public ImageSource? IconImageSource => new ImageSourceConverter().ConvertFrom($"{AppViewModel.WPFApplicationBasePath}Img/ProgressionIcons/{Icon}.png") as ImageSource;

    public LocalizedString Text { get; }
    public LocalizedString? Description { get; }
}