﻿using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

public class ConvertersUtilityViewModel : UtilityViewModel
{
    #region Constructor

    public ConvertersUtilityViewModel()
    {
        Types = new ObservableCollection<ConvertersUtilityTypeViewModel>()
        {
            new ConvertersUtilityCpaGfTypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_GFHeader)),
                modes: new ObservableCollection<SerializableUtilityTypeModeViewModel>()
                {
                    new(CPAGameMode.Rayman2_PC),
                    new(CPAGameMode.Rayman2_Demo1_PC),
                    new(CPAGameMode.Rayman2_Demo2_PC),
                    new(CPAGameMode.Rayman2_iOS),
                    new(CPAGameMode.RaymanM_PC),
                    new(CPAGameMode.RaymanArena_PC),
                    new(CPAGameMode.Rayman3_PC),
                    new(CPAGameMode.TonicTrouble_PC),
                    new(CPAGameMode.TonicTrouble_SE_PC),
                    new(CPAGameMode.DonaldDuck_PC),
                    new(CPAGameMode.PlaymobilHype_PC),
                    new(CPAGameMode.PlaymobilLaura_PC),
                    new(CPAGameMode.PlaymobilAlex_PC),
                    new(CPAGameMode.Dinosaur_PC),
                    new(CPAGameMode.LargoWinch_PC),
                }),

            new ConvertersUtilityUbiArtLocTypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_LOCHeader)),
                modes: new ObservableCollection<SerializableUtilityTypeModeViewModel>()
                {
                    new(UbiArtGameMode.RaymanOrigins_PC, "Rayman Origins"),
                    new(UbiArtGameMode.RaymanOrigins_3DS),
                    new(UbiArtGameMode.RaymanLegends_PC, "Rayman Legends"),
                    new(UbiArtGameMode.RaymanFiestaRun_PC, "Rayman Fiesta Run"),
                }),
        };
        SelectedType = Types.First();

        ConvertCommand = new AsyncRelayCommand(ConvertAsync);
        ConvertBackCommand = new AsyncRelayCommand(ConvertBackAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ConvertCommand { get; }
    public ICommand ConvertBackCommand { get; }

    #endregion

    #region Public Properties

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.Utilities_Converter_Header));
    public override GenericIconKind Icon => GenericIconKind.Utilities_Converters;

    public ObservableCollection<ConvertersUtilityTypeViewModel> Types { get; }
    public ConvertersUtilityTypeViewModel SelectedType { get; set; }

    #endregion

    #region Public Methods

    public async Task ConvertAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            // Attempt to get a default directory
            FileSystemPath defaultDir = SelectedType.SelectedMode.GetDefaultDir(Services.Games);

            // Allow the user to select the files
            FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Converter_FileSelectionHeader,
                DefaultDirectory = defaultDir,
                ExtensionFilter = SelectedType.SourceFileExtension.GetFileFilterItem.ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Allow the user to select the destination directory
            DirectoryBrowserResult destinationResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (destinationResult.CanceledByUser)
                return;

            // Allow the user to select the file extension to export as
            ItemSelectionDialogResult extResult = await Services.UI.SelectItemAsync(new ItemSelectionDialogViewModel(SelectedType.ConvertFormats, Resources.Utilities_Converter_ExportExtensionHeader));

            if (extResult.CanceledByUser)
                return;

            try
            {
                await Task.Run(() =>
                {
                    using RCPContext context = new(fileResult.SelectedFiles.First().Parent);
                    SelectedType.SelectedMode.InitContext(context);

                    // Convert every file
                    foreach (FileSystemPath file in fileResult.SelectedFiles)
                    {
                        // Get the destination file
                        FileSystemPath destinationFile = destinationResult.SelectedDirectory + file.Name;

                        // Set the file extension
                        destinationFile = destinationFile.
                            ChangeFileExtension(new FileExtension(SelectedType.ConvertFormats[extResult.SelectedIndex])).
                            GetNonExistingFileName();

                        // Convert the file
                        SelectedType.Convert(context, file.Name, destinationFile);
                    }
                });

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Converter_Success);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Converting files");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Converter_Error);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ConvertBackAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            // Allow the user to select the files
            FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Converter_FileSelectionHeader,
                ExtensionFilter = new FileFilterItemCollection(SelectedType.ConvertFormats.Select(x => new FileFilterItem($"*{x}", x.ToUpper()))).ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Allow the user to select the destination directory
            DirectoryBrowserResult destinationResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (destinationResult.CanceledByUser)
                return;

            // Get the state
            object? state = await SelectedType.GetConvertBackStateAsync();

            if (state is null)
                return;

            try
            {
                await Task.Run(() =>
                {
                    using RCPContext context = new(destinationResult.SelectedDirectory);
                    SelectedType.SelectedMode.InitContext(context);

                    // Convert every file
                    foreach (FileSystemPath file in fileResult.SelectedFiles)
                    {
                        // Get the destination file
                        FileSystemPath destinationFile = destinationResult.SelectedDirectory + file.Name;

                        // Set the file extension
                        destinationFile = destinationFile.ChangeFileExtension(SelectedType.SourceFileExtension).GetNonExistingFileName();

                        // Convert the file
                        SelectedType.ConvertBack(context, file, destinationFile.Name, state);
                    }
                });

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Converter_Success);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Converting files");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Converter_Error);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        Types.DisposeAll();
    }

    #endregion
}