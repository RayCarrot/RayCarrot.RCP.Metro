using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.OpenSpace;
using BinarySerializer.Ray1;
using BinarySerializer.Ray1.GBA;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class Utility_Serializers_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    public Utility_Serializers_ViewModel()
    {
        Types = new ObservableCollection<Utility_Serializers_TypeViewModel>()
        {
            new Serializers_TypeViewModel<SaveSlot>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R1SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(Ray1GameMode.Rayman1_PC) { Encoder = new PC_SaveEncoder() },
                }),

            new Serializers_TypeViewModel<PC_ConfigFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R1ConfigHeader)),
                fileExtension: new FileExtension(".cfg"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(Ray1GameMode.Rayman1_PC),
                    new(Ray1GameMode.RaymanEducational_PC),
                    new(Ray1GameMode.RaymanDesigner_PC),
                    new(Ray1GameMode.RaymanByHisFans_PC),
                    new(Ray1GameMode.Rayman60Levels_PC),
                }),

            new Serializers_TypeViewModel<R2GeneralSaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R2SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => c.GetRequiredSettings<OpenSpaceSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(CPAGameMode.Rayman2_PC) { Encoder = new PC_SaveEncoder() },
                }),

            new Serializers_TypeViewModel<R2ConfigFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R2ConfigHeader)),
                fileExtension: new FileExtension(".cfg"),
                getEndianFunc: c => c.GetRequiredSettings<OpenSpaceSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(CPAGameMode.Rayman2_PC) { Encoder = new PC_SaveEncoder() },
                }),

            new Serializers_TypeViewModel<RMSaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RMSaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => c.GetRequiredSettings<OpenSpaceSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(CPAGameMode.RaymanM_PC),
                    new(CPAGameMode.RaymanArena_PC),
                }),

            new Serializers_TypeViewModel<R3SaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R3SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => c.GetRequiredSettings<OpenSpaceSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(CPAGameMode.Rayman3_PC) { Encoder = new R3SaveEncoder() },
                }),

            new Serializers_TypeViewModel<RRR_SaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Format_RRRSaveHeader)),
                fileExtension: new FileExtension(".sav"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(JadeGameMode.RaymanRavingRabbids_PC) { Encoder = new RRR_SaveEncoder() },
                }),

            new Serializers_TypeViewModel<Origins_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_ROSaveHeader)),
                fileExtension: new FileExtension(""),
                getEndianFunc: c => c.GetRequiredSettings<UbiArtSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(UbiArtGameMode.RaymanOrigins_PC)
                    {
                        GetDefaultDir = _ => Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins"
                    },
                }),

            new Serializers_TypeViewModel<Legends_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RLSaveHeader)),
                fileExtension: new FileExtension(""),
                getEndianFunc: c => c.GetRequiredSettings<UbiArtSettings>().GetEndian,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(UbiArtGameMode.RaymanLegends_PC)
                    {
                        GetDefaultDir = _ => Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends"
                    },
                }),

            new Serializers_TypeViewModel<JungleRun_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RJRSaveHeader)),
                fileExtension: new FileExtension(".dat"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(UbiArtGameMode.RaymanJungleRun_PC, "Rayman Jungle Run (PC/Android/iOS)")
                    {
                        GetDefaultDir = g =>
                        {
                            var gameSearch = GameSearch.Create(Game.RaymanJungleRun, GamePlatform.WindowsPackage);
                            GameDescriptor? gameDescriptor = g.FindInstalledGame(gameSearch)?.GameDescriptor;

                            if (gameDescriptor is WindowsPackageGameDescriptor winDescr)
                                return winDescr.GetLocalAppDataDirectory();
                            else
                                return FileSystemPath.EmptyPath;
                        }
                    },
                }),

            new Serializers_TypeViewModel<FiestaRun_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Format_RFRSaveHeader)),
                fileExtension: new FileExtension(".dat"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(UbiArtGameMode.RaymanFiestaRun_PC, "Rayman Fiesta Run (PC/Android/iOS)")
                    {
                        GetDefaultDir = g =>
                        {
                            var gameSearch = GameSearch.Create(Game.RaymanFiestaRun, GamePlatform.WindowsPackage);
                            GameDescriptor? gameDescriptor = g.FindInstalledGame(gameSearch)?.GameDescriptor;

                            if (gameDescriptor is WindowsPackageGameDescriptor winDescr)
                                return winDescr.GetLocalAppDataDirectory();
                            else
                                return FileSystemPath.EmptyPath;
                        }
                    },
                }),

            new Serializers_TypeViewModel<SaveData>(
                name: "Rayman Advance Save Data", // TODO-UPDATE: Localize
                fileExtension: null,
                getEndianFunc: _ => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new(Ray1GameMode.Rayman1_GBA) { Encoder = new EEPROMEncoder(0x200) },
                }),

            new Serializers_TypeViewModel<R3GBA_SaveData>(
                name: "Rayman 3 (GBA) Save Data", // TODO-UPDATE: Localize
                fileExtension: null,
                getEndianFunc: _ => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    // TODO-UPDATE: Localize
                    new("Rayman 3 (GBA)") { Encoder = new EEPROMEncoder(0x200) },
                }),

            new Serializers_TypeViewModel<RHR_SaveData>(
                name: "Rayman Hoodlums' Revenge Save Data", // TODO-UPDATE: Localize
                fileExtension: null,
                getEndianFunc: _ => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    // TODO-UPDATE: Localize
                    new("Rayman Hoodlums' Revenge") { Encoder = new EEPROMEncoder(0x200) },
                }),

            new Serializers_TypeViewModel<RRRGBA_SaveData>(
                name: "Rayman Raving Rabbids (GBA) Save Data", // TODO-UPDATE: Localize
                fileExtension: null,
                getEndianFunc: _ => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    // TODO-UPDATE: Localize
                    new("Rayman Raving Rabbids (GBA)") { Encoder = new EEPROMEncoder(0x200) },
                }),

            new Serializers_TypeViewModel<Unity_PlayerPrefs>(
                name: new ResourceLocString(nameof(Resources.Utilities_Format_UnityPlayerPrefsHeader)),
                fileExtension: new FileExtension(".dat"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new("Unity")
                    {
                        GetDefaultDir = g =>
                        {
                            var gameSearch = GameSearch.Create(Game.RabbidsBigBang, GamePlatform.WindowsPackage);
                            GameDescriptor? gameDescriptor = g.FindInstalledGame(gameSearch)?.GameDescriptor;

                            if (gameDescriptor is WindowsPackageGameDescriptor winDescr)
                                return winDescr.GetLocalAppDataDirectory();
                            else
                                return FileSystemPath.EmptyPath;
                        }
                    },
                }),

            new Serializers_TypeViewModel<GameMaker_DSMap>(
                name: new ResourceLocString(nameof(Resources.Utilities_Format_GameMakerDSMapHeader)),
                fileExtension: new FileExtension(".txt"),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new("Game Maker")
                    {
                        Encoder = new GameMaker_HexStringEncoder(),
                        GetDefaultDir = _ => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "RaymanRedemption"
                    },
                }),

            new Serializers_TypeViewModel<PatchFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Format_GamePatchHeader)),
                fileExtension: new FileExtension(PatchFile.FileExtension),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new("Rayman Control Panel"),
                }),

            new Serializers_TypeViewModel<PatchLibraryFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Format_GamePatchLibraryHeader)),
                fileExtension: new FileExtension(PatchLibraryFile.FileExtension),
                getEndianFunc: c => Endian.Little,
                modes: new ObservableCollection<Utility_SerializableTypeModeViewModel>()
                {
                    new("Rayman Control Panel"),
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
            FileSystemPath defaultDir = SelectedType.SelectedMode.GetDefaultDir(Services.Games);

            // Allow the user to select the files
            FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Serializers_FileSelectionHeader,
                DefaultDirectory = defaultDir,
                ExtensionFilter = SelectedType.FileExtension?.GetFileFilterItem.ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser || !fileResult.SelectedFiles.Any())
                return;

            try
            {
                await Task.Run(() =>
                {
                    MemorySerializerLogger log = new();

                    using RCPContext context = new(fileResult.SelectedFiles.First().Parent, logger: log);
                    SelectedType.SelectedMode.InitContext(context);

                    try
                    {
                        // Deserialize every file
                        foreach (FileSystemPath file in fileResult.SelectedFiles)
                            SelectedType.Deserialize(context, file.Name);
                    }
                    finally
                    {
                        Log = log.GetString();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Logging serialized files");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Serializers_LogError);
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
            FileSystemPath defaultDir = SelectedType.SelectedMode.GetDefaultDir(Services.Games);

            // Allow the user to select the files
            FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Serializers_FileSelectionHeader,
                DefaultDirectory = defaultDir,
                ExtensionFilter = SelectedType.FileExtension?.GetFileFilterItem.ToString(),
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

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Serializers_DeserializeSuccess);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Deserializing files");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Serializers_DeserializeError);
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
                Title = Resources.Utilities_Serializers_FileSelectionHeader,
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
                        if (SelectedType.FileExtension != null)
                            destinationFile = destinationFile.ChangeFileExtension(SelectedType.FileExtension);
                        
                        destinationFile = destinationFile.GetNonExistingFileName();

                        // Deserialize the file JSON
                        object data = JsonHelpers.DeserializeFromFile(file, SelectedType.Type);

                        // Serialize the data to the destination file
                        SelectedType.Serialize(context, destinationFile.Name, data);
                    }
                });

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Serializers_SerializeSuccess);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Serializing files");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Serializers_SerializeError);
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