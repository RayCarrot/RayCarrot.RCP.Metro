using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Infralution.Localization.Wpf;
using MahApps.Metro.Controls;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;

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
            Icon = new ImageSourceConverter().ConvertFromString(AppViewModel.ApplicationBasePath + "/Img/Rayman Control Panel Icon.ico") as ImageSource;
            IconBitmapScalingMode = BitmapScalingMode.NearestNeighbor;

            // Set localization source
            ResxExtension.SetDefaultResxName(this, AppLanguages.ResourcePath);

            if (RCF.IsBuilt)
            {
                // Set transition
                WindowTransitionsEnabled = RCFRCP.Data?.EnableAnimations ?? true;

                RCFCore.Logger?.LogInformationSource($"The window {this} has been created");
            }
        }

        /// <summary>
        /// Shows the <see cref="Window"/> as a dialog
        /// </summary>
        public new void ShowDialog()
        {
            // Set owner window
            Owner = Application.Current?.Windows.Cast<Window>().FindItem(x => x.IsActive);

            // Set startup location
            WindowStartupLocation = Owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;

            // Show the window as a dialog
            base.ShowDialog();
        }
    }
}