using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a progression info item
    /// </summary>
    public class ProgressionInfoItemViewModel : BaseRCPViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="icon">The icon</param>
        /// <param name="content">The content</param>
        public ProgressionInfoItemViewModel(ProgressionIcons icon, string content)
        {
            Icon = icon;
            Content = content;
        }

        /// <summary>
        /// The icon
        /// </summary>
        public ProgressionIcons Icon { get; }

        /// <summary>
        /// The icon as an image source
        /// </summary>
        public ImageSource IconImageSource => new ImageSourceConverter().ConvertFrom($"{AppViewModel.ApplicationBasePath}Img/ProgressionIcons/UbiArt/{Icon}.png") as ImageSource;

        /// <summary>
        /// The content
        /// </summary>
        public string Content { get; }
    }
}