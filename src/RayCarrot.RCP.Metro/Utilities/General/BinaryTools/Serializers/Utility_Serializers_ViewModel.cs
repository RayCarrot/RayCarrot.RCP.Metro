using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using BinarySerializer.Ray1;
using BinarySerializer.UbiArt;
using NLog;

namespace RayCarrot.RCP.Metro;

public class Utility_Serializers_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    public Utility_Serializers_ViewModel()
    {
        // TODO-UPDATE: Localize & include file ext in all the names
        Types = new ObservableCollection<Utility_Serializers_TypeViewModel>()
        {
            new Serializers_TypeViewModel<PC_SaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R1SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(Ray1GameMode.Rayman1_PC) { Encoder = new PC_SaveEncoder() },
                }),

            new Serializers_TypeViewModel<PC_ConfigFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R1ConfigHeader)),
                fileExtension: new FileExtension(".cfg"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(Ray1GameMode.Rayman1_PC),
                    new Utility_SerializableTypeModeViewModel(Ray1GameMode.RaymanEducational_PC),
                    new Utility_SerializableTypeModeViewModel(Ray1GameMode.RaymanDesigner_PC),
                    new Utility_SerializableTypeModeViewModel(Ray1GameMode.RaymanByHisFans_PC),
                    new Utility_SerializableTypeModeViewModel(Ray1GameMode.Rayman60Levels_PC),
                }),

            new Serializers_TypeViewModel<R2GeneralSaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R2SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => c.GetSettings<OpenSpaceSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Rayman2_PC) { Encoder = new PC_SaveEncoder() },
                }),

            new Serializers_TypeViewModel<R2ConfigFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R2ConfigHeader)),
                fileExtension: new FileExtension(".cfg"),
                getEndianFunc: c => c.GetSettings<OpenSpaceSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Rayman2_PC) { Encoder = new PC_SaveEncoder() },
                }),

            new Serializers_TypeViewModel<RMSaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RMSaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => c.GetSettings<OpenSpaceSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.RaymanM_PC),
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.RaymanArena_PC),
                }),

            new Serializers_TypeViewModel<R3SaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R3SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => c.GetSettings<OpenSpaceSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(OpenSpaceGameMode.Rayman3_PC) { Encoder = new R3SaveEncoder() },
                }),

            new Serializers_TypeViewModel<RRR_SaveFile>(
                name: new ConstLocString("Rayman Raving Rabbids Save Files (.sav)"),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel("Rayman Raving Rabbids (PC)", Games.RaymanRavingRabbids) { Encoder = new RRR_SaveEncoder() },
                }),

            new Serializers_TypeViewModel<Origins_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_ROSaveHeader)),
                fileExtension: new FileExtension(""),
                getEndianFunc: c => c.GetSettings<UbiArtSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(UbiArtGameMode.RaymanOrigins_PC)
                    {
                        GetDefaultDir = () => Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins"
                    },
                }),

            new Serializers_TypeViewModel<Legends_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RLSaveHeader)),
                fileExtension: new FileExtension(""),
                getEndianFunc: c => c.GetSettings<UbiArtSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(UbiArtGameMode.RaymanLegends_PC)
                    {
                        GetDefaultDir = () => Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends"
                    },
                }),

            new Serializers_TypeViewModel<JungleRun_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RJRSaveHeader)),
                fileExtension: new FileExtension(".dat"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(UbiArtGameMode.RaymanJungleRun_PC, "Rayman Jungle Run (PC/Android/iOS)")
                    {
                        GetDefaultDir = () => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() +
                                              "Packages" +
                                              Games.RaymanJungleRun.GetManager<GameManager_WinStore>().FullPackageName +
                                              "LocalState"
                    },
                }),

            new Serializers_TypeViewModel<FiestaRun_SaveData>(
                name: new ConstLocString("Rayman Fiesta Run Save File"),
                fileExtension: new FileExtension(".dat"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel(UbiArtGameMode.RaymanFiestaRun_PC, "Rayman Fiesta Run (PC/Android/iOS)")
                    {
                        GetDefaultDir = () => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() +
                                              "Packages" +
                                              Games.RaymanFiestaRun.GetManager<GameManager_WinStore>().FullPackageName +
                                              "LocalState"
                    },
                }),

            new Serializers_TypeViewModel<Unity_PlayerPrefs>(
                name: new ConstLocString("Unity Player Preferences / Rabbids Big Bang Save File (.dat)"),
                fileExtension: new FileExtension(".dat"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel("Unity")
                    {
                        GetDefaultDir = () => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + 
                                              "Packages" + 
                                              Games.RabbidsBigBang.GetManager<GameManager_WinStore>().FullPackageName + 
                                              "LocalState"
                    },
                }),

            new Serializers_TypeViewModel<GameMaker_DSMap>(
                name: new ConstLocString("Game Maker DS Map / Rayman Redemption Save File (.txt)"),
                fileExtension: new FileExtension(".txt"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new Utility_SerializableTypeModeViewModel("Game Maker")
                    {
                        Encoder = new GameMaker_HexStringEncoder(),
                        GetDefaultDir = () => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "RaymanRedemption"
                    },
                }),
        };

        SelectedType = Types.First();

        DeserializeCommand = new AsyncRelayCommand(DeserializeAsync);
        SerializeCommand = new AsyncRelayCommand(SerializeAsync);
        LogCommand = new AsyncRelayCommand(LogAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand LogCommand { get; }
    public ICommand DeserializeCommand { get; }
    public ICommand SerializeCommand { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<Utility_Serializers_TypeViewModel> Types { get; }
    public Utility_Serializers_TypeViewModel SelectedType { get; set; }

    public bool IsLoading { get; set; }
    public string? Log { get; set; }

    #endregion

    #region Public Methods

    public async Task LogAsync()
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
                // TODO-UPDATE: Update localization
                Title = Resources.Utilities_Converter_FileSelectionHeader,
                DefaultDirectory = defaultDir?.FullPath,
                ExtensionFilter = SelectedType.FileExtension.GetFileFilterItem.ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser || !fileResult.SelectedFiles.Any())
                return;

            try
            {
                await Task.Run(() =>
                {
                    MemorySerializerLog log = new();

                    using RCPContext context = new(fileResult.SelectedFiles.First().Parent, log: log);
                    SelectedType.SelectedMode.InitContext(context);

                    try
                    {
                        // Deserialize every file
                        foreach (FileSystemPath file in fileResult.SelectedFiles)
                            SelectedType.Deserialize(context, file.Name);
                    }
                    finally
                    {
                        Log = log.GetString;
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Logging serialized files");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "The files could not be logged");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeserializeAsync()
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
                // TODO-UPDATE: Update localization
                Title = Resources.Utilities_Converter_FileSelectionHeader,
                DefaultDirectory = defaultDir?.FullPath,
                ExtensionFilter = SelectedType.FileExtension.GetFileFilterItem.ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser || !fileResult.SelectedFiles.Any())
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
                    SelectedType.SelectedMode.InitContext(context);

                    // Deserialize every file
                    foreach (FileSystemPath file in fileResult.SelectedFiles)
                    {
                        object? fileObj = SelectedType.Deserialize(context, file.Name);

                        if (fileObj is null)
                        {
                            Logger.Warn("Deserialized file was null");
                            continue;
                        }

                        // Get the destination file
                        FileSystemPath destinationFile = destinationResult.SelectedDirectory + file.Name;

                        // Set the file extension
                        destinationFile = destinationFile.ChangeFileExtension(new FileExtension(".json")).GetNonExistingFileName();

                        // Serialize as JSON
                        JsonHelpers.SerializeToFile(fileObj, destinationFile);
                    }
                });

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The files were successfully deserialized");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Deserializing files");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "The files could not be deserialized");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SerializeAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            // Allow the user to select the files
            FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                // TODO-UPDATE: Update localization
                Title = Resources.Utilities_Converter_FileSelectionHeader,
                ExtensionFilter = new FileFilterItem("*.json", "JSON").ToString(),
                MultiSelection = true,
            });

            if (fileResult.CanceledByUser || !fileResult.SelectedFiles.Any())
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
                    using RCPContext context = new(destinationResult.SelectedDirectory);
                    SelectedType.SelectedMode.InitContext(context);

                    // Serialize every file
                    foreach (FileSystemPath file in fileResult.SelectedFiles)
                    {
                        // Get the destination file
                        FileSystemPath destinationFile = destinationResult.SelectedDirectory + file.Name;

                        // Set the file extension
                        destinationFile = destinationFile.ChangeFileExtension(SelectedType.FileExtension).GetNonExistingFileName();

                        // Deserialize the file JSON
                        object data = JsonHelpers.DeserializeFromFile(file, SelectedType.Type);

                        // Serialize the data to the destination file
                        SelectedType.Serialize(context, destinationFile.Name, data);
                    }
                });

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The files were successfully serialized");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Serializing files");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "The files could not be serialized");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        Log = null;
        Types.DisposeAll();
    }

    #endregion
}