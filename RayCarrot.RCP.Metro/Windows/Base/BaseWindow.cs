using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
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
            // Set minimum size
            MinWidth = 400;
            MinHeight = 300;

            // Set title style
            TitleCharacterCasing = CharacterCasing.Normal;

            // Set icon style
            Icon = new ImageSourceConverter().ConvertFromString(AppHandler.ApplicationBasePath + "/Img/Rayman Control Panel Icon.ico") as ImageSource;
            IconBitmapScalingMode = BitmapScalingMode.NearestNeighbor;

            // Set owner window
            Owner = Application.Current?.Windows.Cast<Window>().FindItem(x => x.IsActive);

            // Set startup location
            WindowStartupLocation = Owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;

        }
    }
}