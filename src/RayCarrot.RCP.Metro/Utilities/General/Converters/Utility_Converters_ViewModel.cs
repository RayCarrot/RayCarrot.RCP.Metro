using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.UbiArt;
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
            new Utility_Converters_UbiArtLoc_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_LOCHeader)),
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman Origins"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new UbiArtSettings(EngineVersion.RaymanOrigins, Platform.PC),
                        Game = Games.RaymanOrigins,
                        Endian = Endian.Big,
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman Origins (3DS)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new UbiArtSettings(EngineVersion.RaymanOrigins, Platform.Nintendo3DS),
                        Endian = Endian.Little,
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman Legends"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new UbiArtSettings(EngineVersion.RaymanLegends, Platform.PC),
                        Game = Games.RaymanLegends,
                        Endian = Endian.Big,
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman Fiesta Run"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new UbiArtSettings(EngineVersion.RaymanFiestaRun, Platform.PC),
                        Game = Games.RaymanFiestaRun,
                        Endian = Endian.Big,
                    }),
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
            FileSystemPath? defaultDir = SelectedType.SelectedMode.Data.GetDefaultDir();

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
                    SelectedType.SelectedMode.Data.InitContext(context);

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

            try
            {
                await Task.Run(() =>
                {
                    using RCPContext context = new(fileResult.SelectedFiles.First().Parent);
                    SelectedType.SelectedMode.Data.InitContext(context);

                    // Convert every file
                    foreach (FileSystemPath file in fileResult.SelectedFiles)
                    {
                        // Get the destination file
                        FileSystemPath destinationFile = destinationResult.SelectedDirectory + file.Name;

                        // Set the file extension
                        destinationFile = destinationFile.ChangeFileExtension(SelectedType.SourceFileExtension).GetNonExistingFileName();

                        // Convert the file
                        SelectedType.ConvertBack(context, file, destinationFile.Name);
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