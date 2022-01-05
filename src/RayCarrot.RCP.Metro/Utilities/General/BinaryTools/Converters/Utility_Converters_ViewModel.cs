using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class Utility_Converters_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    public Utility_Converters_ViewModel()
    {
        Types = new ObservableCollection<Utility_Converters_TypeViewModel>()
        {
            new Utility_Converters_OpenSpaceGF_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_GFHeader)),
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Rayman2_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Rayman2_Demo1_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Rayman2_Demo2_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Rayman2_iOS),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.RaymanM_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.RaymanArena_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Rayman3_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.TonicTrouble_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.TonicTrouble_SE_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.DonaldDuck_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.PlaymobilHype_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.PlaymobilLaura_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.PlaymobilAlex_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Dinosaur_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.LargoWinch_PC),
                }),

            new Utility_Converters_UbiArtLoc_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_LOCHeader)),
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(UbiArtGameMode.RaymanOrigins_PC, "Rayman Origins"),
                    new Utility_SerializableTypeModeViewModel(UbiArtGameMode.RaymanOrigins_3DS),
                    new Utility_SerializableTypeModeViewModel(UbiArtGameMode.RaymanLegends_PC, "Rayman Legends"),
                    new Utility_SerializableTypeModeViewModel(UbiArtGameMode.RaymanFiestaRun_PC, "Rayman Fiesta Run"),
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

    public ObservableCollection<Utility_Converters_TypeViewModel> Types { get; }
    public Utility_Converters_TypeViewModel SelectedType { get; set; }

    public bool IsLoading { get; set; }

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
            FileSystemPath? defaultDir = SelectedType.SelectedMode.GetDefaultDir();

            // Make sure the directory exists
            if (defaultDir?.DirectoryExists != true)
                defaultDir = null;

            // Allow the user to select the files
            FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Converter_FileSelectionHeader,
                DefaultDirectory = defaultDir?.FullPath,
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
            FileExtensionSelectionDialogResult extResult = await Services.UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(SelectedType.ConvertFormats, Resources.Utilities_Converter_ExportExtensionHeader));

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
                        destinationFile = destinationFile.ChangeFileExtension(new FileExtension(extResult.SelectedFileFormat)).GetNonExistingFileName();

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
                    using RCPContext context = new(fileResult.SelectedFiles.First().Parent);
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

    public void Dispose()
    {
        Types.DisposeAll();
    }

    #endregion
}