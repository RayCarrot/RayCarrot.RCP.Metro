using System.Windows;
using System.Windows.Controls;
using RayCarrot.RCP.Metro.Pages.Settings.Sections;

namespace RayCarrot.RCP.Metro.Pages.Settings;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : BasePage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private bool _isScrolling;

    private void SectionButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: SettingsSectionViewModel section } ||
            DataContext is not SettingsPageViewModel viewModel)
            return;

        if (viewModel.Sections.First().Contains(section))
            SectionsScrollViewer.ScrollToTop();
        else
            // Scroll all the way down and then up. This places the item we scroll to on the top rather than the bottom.
            SectionsScrollViewer.ScrollToBottom();

        section.IsSelected = true;
    }

    private void SectionsScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel viewModel)
            return;

        _isScrolling = true;

        try
        {
            foreach (SettingsSectionViewModel section in viewModel.FlatSections)
                section.IsSelected = false;

            // Find the first visible collection of section
            ObservableCollection<SettingsSectionViewModel>? sections = SectionsItemsControl.Items.
                Cast<ObservableCollection<SettingsSectionViewModel>>().
                FirstOrDefault(x => ((FrameworkElement)SectionsItemsControl.ItemContainerGenerator.ContainerFromItem(x)).IsUserVisible(SectionsScrollViewer));

            if (sections == null)
                return;

            // Select the sections
            foreach (SettingsSectionViewModel section in sections)
                section.IsSelected = true;

            // If we're scrolled to the bottom we also select all subsequent sections
            if (Math.Abs(SectionsScrollViewer.VerticalOffset - SectionsScrollViewer.ScrollableHeight) < 1.0)
            {
                bool foundSection = false;

                foreach (SettingsSectionViewModel section in viewModel.FlatSections)
                {
                    if (!foundSection)
                    {
                        if (section == sections.Last())
                            foundSection = true;
                    }
                    else
                    {
                        section.IsSelected = true;
                    }
                }
            }
        }
        finally
        {
            _isScrolling = false;
        }
    }

    private void SectionGroupBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: SettingsSectionViewModel section } element)
            return;

        section.PropertyChanged += (_, ee) =>
        {
            if (!_isScrolling && ee.PropertyName == nameof(SettingsSectionViewModel.IsSelected))
            {
                element.BringIntoView();
                element.Focus();
            }
        };
    }
}