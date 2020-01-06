using RayCarrot.CarrotFramework.Abstractions;
using System.Windows.Controls;
using System.Windows.Input;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for HelpPage.xaml
    /// </summary>
    public partial class HelpPage : BasePage<HelpPageViewModel>
    {
        public HelpPage()
        {
            InitializeComponent();

            RCFCore.Data.UserLevelChanged += (s, e) =>
            {
                if ((HelpTreeView.SelectedItem as HelpItemViewModel)?.RequiredUserLevel > RCFRCP.Data.UserLevel)
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