using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Page_Help_UI.xaml
    /// </summary>
    public partial class Page_Help_UI : BasePage<Page_Help_ViewModel>
    {
        public Page_Help_UI()
        {
            InitializeComponent();

            WPF.Services.Data.UserLevelChanged += (s, e) =>
            {
                if ((HelpTreeView.SelectedItem as Page_Help_ItemViewModel)?.RequiredUserLevel > RCPServices.Data.UserLevel)
                    ((TreeViewItem)HelpTreeView.ItemContainerGenerator.ContainerFromItem(HelpTreeView.Items[0])).IsSelected = true;
            };
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Redirect vertical scrolling to parent scroll viewer
            MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset + (e.Delta * -0.5));
        }
    }
}