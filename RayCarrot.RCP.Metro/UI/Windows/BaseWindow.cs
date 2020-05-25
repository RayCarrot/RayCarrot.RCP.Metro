using MahApps.Metro.Controls;
using RayCarrot.Common;
using RayCarrot.Logging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            RL.Logger?.LogInformationSource($"A window is being created...");

            // Default to true
            CloseWithEscape = true;

            // Set minimum size
            MinWidth = 400;
            MinHeight = 300;
            
            // Set title style
            TitleCharacterCasing = CharacterCasing.Normal;

            // Set icon style
            Icon = new ImageSourceConverter().ConvertFromString(AppViewModel.WPFApplicationBasePath + "Img/RCP_Metro.ico") as ImageSource;
            IconBitmapScalingMode = BitmapScalingMode.NearestNeighbor;

            // Set owner window
            Owner = Application.Current?.Windows.Cast<Window>().FindItem(x => x.IsActive);

            RL.Logger?.LogInformationSource($"The owner window has been set to {Owner?.ToString() ?? "null"}");

            // Do not show in the task bar if the window has a owner, is not the main window and a main window has been created
            if (Owner != null && Application.Current?.MainWindow != null && this != Application.Current.MainWindow)
                ShowInTaskbar = false;

            // Due to a WPF glitch the main window needs to be focused upon closing
            Closed += (s, e) =>
            {
                if (this != Application.Current.MainWindow)
                    Application.Current.MainWindow?.Focus();
            };

            // Set transition
            WindowTransitionsEnabled = RCPServices.Data?.EnableAnimations ?? true;

            RL.Logger?.LogInformationSource($"The window {this} has been created");

            PreviewKeyDown += (s, e) =>
            {
                if (CloseWithEscape && e.Key == Key.Escape)
                    Close();
            };
        }

        /// <summary>
        /// Shows the <see cref="Window"/> as a dialog
        /// </summary>
        public new void ShowDialog()
        {
            // Set startup location
            WindowStartupLocation = Owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;

            // Show the window as a dialog
            base.ShowDialog();
        }

        /// <summary>
        /// Indicates if the escape key can be used to close the window
        /// </summary>
        public bool CloseWithEscape { get; set; }
    }
}