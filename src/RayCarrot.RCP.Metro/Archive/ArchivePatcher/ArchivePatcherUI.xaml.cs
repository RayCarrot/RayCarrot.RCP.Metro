﻿using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// Interaction logic for ArchivePatcherUI.xaml
/// </summary>
public partial class ArchivePatcherUI : WindowContentControl
{
    #region Constructor
    
    public ArchivePatcherUI(ArchivePatcherViewModel viewModel)
    {
        DataContext = viewModel;
        ViewModel = viewModel;

        Loaded += ArchivePatcherUI_OnLoaded;

        // Set up UI
        InitializeComponent();
    }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    public ArchivePatcherViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Archive Patcher"; // TODO-UPDATE: Localize
        WindowInstance.Icon = GenericIconKind.Window_ArchivePatcher;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 900;
        WindowInstance.Height = 600;
    }

    protected override async Task<bool> ClosingAsync()
    {
        if (!await base.ClosingAsync())
            return false;

        // Cancel the closing if it's loading
        return !ViewModel.LoadOperation.IsLoading;
    }

    #endregion

    #region Event Handlers

    private async void ArchivePatcherUI_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ArchivePatcherUI_OnLoaded;

        await ViewModel.LoadPatchesAsync();
    }

    private void PatchesItemsControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r.VisualHit.GetType() != typeof(ListBoxItem))
            ViewModel.DeselectAll();
    }

    private void PatchesListBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Redirect the mouse wheel movement to allow scrolling
        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = e.Source
        };

        PatchesScrollViewer?.RaiseEvent(eventArg);
        e.Handled = true;
    }

    private void PatchesListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        var container = (PatchContainerViewModel)listBox.DataContext;
        var dropHandler = (PatchDropHandler)DragDrop.GetDropHandler(listBox);
        dropHandler.ViewModel = container;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    private async void ApplyButton_OnClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.ApplyAsync();

        // Close the dialog
        WindowInstance.Close();
    }

    #endregion
}