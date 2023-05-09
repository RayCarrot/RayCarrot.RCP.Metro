using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Patcher
{
    /// <summary>
    /// Interaction logic for PatchCreatorFileModificationsPage.xaml
    /// </summary>
    public partial class PatchCreatorFileModificationsPage : UserControl
    {
        public PatchCreatorFileModificationsPage()
        {
            InitializeComponent();
        }

        public PatchCreatorViewModel ViewModel => (PatchCreatorViewModel)DataContext;

        private void FilesGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.SelectedFile = null;
        }
    }
}
