using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for FirstLaunchInfoDialog.xaml
    /// </summary>
    public partial class FirstLaunchInfoDialog : BaseWindow
    {
        public FirstLaunchInfoDialog()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}