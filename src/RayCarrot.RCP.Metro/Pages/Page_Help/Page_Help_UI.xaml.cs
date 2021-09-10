using System.Windows.Controls;

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

            Services.InstanceData.UserLevelChanged += (s, e) =>
            {
                if ((HelpTreeView.SelectedItem as Page_Help_ItemViewModel)?.RequiredUserLevel > Services.Data.App_UserLevel)
                    ((TreeViewItem)HelpTreeView.ItemContainerGenerator.ContainerFromItem(HelpTreeView.Items[0])).IsSelected = true;
            };
        }
    }
}