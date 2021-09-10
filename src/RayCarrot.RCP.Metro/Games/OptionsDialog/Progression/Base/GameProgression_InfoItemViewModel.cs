using System;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a progression info item
    /// </summary>
    public class GameProgression_InfoItemViewModel : BaseRCPViewModel, IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="icon">The icon</param>
        /// <param name="content">The content</param>
        /// <param name="description">Optional additional description</param>
        public GameProgression_InfoItemViewModel(GameProgression_Icon icon, LocalizedString content, LocalizedString description = null)
        {
            Icon = icon;
            Content = content;
            Description = description ?? new ConstLocString(null);
        }

        /// <summary>
        /// The icon
        /// </summary>
        public GameProgression_Icon Icon { get; }

        /// <summary>
        /// The icon as an image source
        /// </summary>
        public ImageSource IconImageSource => new ImageSourceConverter().ConvertFrom($"{AppViewModel.WPFApplicationBasePath}Img/ProgressionIcons/{Icon}.png") as ImageSource;

        /// <summary>
        /// The content
        /// </summary>
        public LocalizedString Content { get; }

        /// <summary>
        /// Optional additional description
        /// </summary>
        public LocalizedString Description { get; }

        public void Dispose()
        {
            Content?.Dispose();
            Description?.Dispose();
        }
    }
}