﻿using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// Interaction logic for PatchCreatorDialog.xaml
/// </summary>
public partial class PatchCreatorDialog : WindowContentControl
{
    #region Constructor
    
    public PatchCreatorDialog(PatchCreatorViewModel viewModel, FileSystemPath? existingPatch)
    {
        DataContext = viewModel;
        ViewModel = viewModel;

        _patchToImportFrom = existingPatch;

        // Set up UI
        InitializeComponent();

        Loaded += PatchCreatorUI_Loaded;
    }

    private async void PatchCreatorUI_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PatchCreatorUI_Loaded;

        if (_patchToImportFrom == null)
            return;

        bool success = await ViewModel.ImportFromPatchAsync(_patchToImportFrom.Value);

        if (!success)
            WindowInstance.Close();
    }

    #endregion

    #region Private Fields

    private readonly FileSystemPath? _patchToImportFrom;

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    public PatchCreatorViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Patch Creator"; // TODO-UPDATE: Localize
        WindowInstance.Icon = GenericIconKind.Window_PatchCreator;
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

    #region Public Methods

    public override void Dispose()
    {
        base.Dispose();

        DataContext = null;
        ViewModel.Dispose();
    }

    #endregion

    #region Event Handlers

    private void FilesGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        ViewModel.SelectedFile = null;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    private async void CreateButton_OnClick(object sender, RoutedEventArgs e)
    {
        bool result = await ViewModel.CreatePatchAsync();
        
        if (!result)
            return;
        
        WindowInstance.Close();
    }

    #endregion
}