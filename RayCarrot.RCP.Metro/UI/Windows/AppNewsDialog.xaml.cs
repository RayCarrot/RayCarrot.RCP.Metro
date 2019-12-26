using System.Windows;
using RayCarrot.RCP.Core.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for AppNewsDialog.xaml
    /// </summary>
    public partial class AppNewsDialog : BaseWindow
    {
        public AppNewsDialog()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}