﻿using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for JumplistEditDialog.xaml
/// </summary>
public partial class JumpListEditDialog : WindowContentControl, IDialogWindowControl<JumpListEditViewModel, JumpListEditResult>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="vm">The view model</param>
    public JumpListEditDialog(JumpListEditViewModel vm)
    {
        InitializeComponent();
        ViewModel = vm;
        DataContext = ViewModel;
        CanceledByUser = true;
    }

    #endregion

    #region Private Fields

    private bool _hasLoaded;

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the dialog was canceled by the user, default is true
    /// </summary>
    public bool CanceledByUser { get; set; }

    /// <summary>
    /// The view model
    /// </summary>
    public JumpListEditViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Icon = GenericIconKind.Window_JumpListEdit;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current result for the dialog
    /// </summary>
    /// <returns>The result</returns>
    public JumpListEditResult GetResult()
    {
        return new JumpListEditResult(ViewModel.Included.ToArray(), ViewModel.AutoSort)
        {
            CanceledByUser = CanceledByUser
        };
    }

    #endregion

    #region Event Handlers

    private async void JumpListEditDialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_hasLoaded)
            return;

        _hasLoaded = true;

        await ViewModel.LoadIconsAsync();
    }

    private void ListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        var dropHandler = (JumpListEditDropHandler)GongSolutions.Wpf.DragDrop.DragDrop.GetDropHandler(listBox);
        dropHandler.ViewModel = ViewModel;
    }

    private void ListBoxItem_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is ListBoxItem item)
            item.IsSelected = false;
    }

    private void ListBoxItem_OnSelected(object sender, RoutedEventArgs e)
    {
        if (sender is ListBoxItem item)
            item.Focus();
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        CanceledByUser = false;

        // Close the dialog
        WindowInstance.Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Close the dialog
        WindowInstance.Close();
    }

    #endregion
}