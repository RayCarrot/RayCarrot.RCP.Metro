using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Media;

namespace RayCarrot.RCP.Updater
{
    /// <summary>
    /// A base window to inherit from
    /// </summary>
    public class BaseWindow : MetroWindow
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseWindow()
        {
            // Set title style
            TitleCharacterCasing = CharacterCasing.Normal;

            // Disable maximize button
            ShowMaxRestoreButton = false;

            // Set icon style
            Icon = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RayCarrot.RCP.Updater;component/Img/Rayman Control Panel Icon.ico") as ImageSource;
            IconBitmapScalingMode = BitmapScalingMode.NearestNeighbor;
        }
    }
}