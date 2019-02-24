using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for BackupPage.xaml
    /// </summary>
    public partial class BackupPage : BaseUserControl<BackupPageViewModel>
    {
        public BackupPage()
        {
            InitializeComponent();
        }

        private void BackupPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.RefresCommand.Execute(null);
        }
    }
}