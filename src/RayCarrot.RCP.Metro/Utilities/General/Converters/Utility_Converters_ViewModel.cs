using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using BinarySerializer.UbiArt;
using NLog;
using RayCarrot.IO;
using Platform = BinarySerializer.UbiArt.Platform;

namespace RayCarrot.RCP.Metro;

public class Utility_Converters_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    public Utility_Converters_ViewModel()
    {
        Types = new ObservableCollection<Utility_Converters_TypeViewModel>()
        {
            // TODO-UPDATE: Set game
            new Utility_Converters_OpenSpaceGF_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_GFHeader)),
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman 2 (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.Rayman2, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman 2 Demo (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.Rayman2Demo, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman 2 (iOS)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.Rayman2, 
                            platform: BinarySerializer.OpenSpace.Platform.iOS),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman M/Arena (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.RaymanM, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman 3 (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.Rayman3, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Tonic Trouble (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.TonicTrouble, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Tonic Trouble Special Edition (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.TonicTroubleSpecialEdition, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Donald Duck: Quack Attack (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.DonaldDuckQuackAttack, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Playmobil: Hype (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.PlaymobilHype, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Playmobil: Laura (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.PlaymobilLaura, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Playmobil: Alex (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.PlaymobilAlex, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Disney's Dinosaur (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.Dinosaur, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Largo Winch (PC)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new OpenSpaceSettings(
                            engineVersion: BinarySerializer.OpenSpace.EngineVersion.LargoWinch, 
                            platform: BinarySerializer.OpenSpace.Platform.PC),
                    }),
                }),

            new Utility_Converters_UbiArtLoc_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_LOCHeader)),
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman Origins"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new UbiArtSettings(Game.RaymanOrigins, Platform.PC),
                        Game = Games.RaymanOrigins,
                        Endian = Endian.Big,
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman Origins (3DS)"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new UbiArtSettings(Game.RaymanOrigins, Platform.Nintendo3DS),
                        Endian = Endian.Little,
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman Legends"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new UbiArtSettings(Game.RaymanLegends, Platform.PC),
                        Game = Games.RaymanLegends,
                        Endian = Endian.Big,
                    }),
                    new Utility_SerializableTypeModeViewModel(new ConstLocString("Rayman Fiesta Run"), new Utility_SerializableTypeModeData
                    {
                        GetSettings = () => new UbiArtSettings(Game.RaymanFiestaRun, Platform.PC),
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

            // Get the state
            object? state = await SelectedType.GetConvertBackStateAsync();

            if (state is null)
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