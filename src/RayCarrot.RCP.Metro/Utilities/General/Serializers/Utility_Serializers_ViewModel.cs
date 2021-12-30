﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using BinarySerializer.Ray1;
using BinarySerializer.UbiArt;
using NLog;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class Utility_Serializers_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    public Utility_Serializers_ViewModel()
    {
        Utility_Serializers_TypeModeData mode_Ray1_PC = new()
        {
            InitContext = x => x.AddSettings(new Ray1Settings(Ray1EngineVersion.PC)),
            Game = Games.Rayman1,
        };
        Utility_Serializers_TypeModeData mode_Ray1_PC_Edu = new()
        {
            InitContext = x => x.AddSettings(new Ray1Settings(Ray1EngineVersion.PC_Edu)),
            Game = Games.EducationalDos,
        };
        Utility_Serializers_TypeModeData mode_Ray1_PC_Kit = new()
        {
            InitContext = x => x.AddSettings(new Ray1Settings(Ray1EngineVersion.PC_Kit)),
            Game = Games.RaymanDesigner,
        };
        Utility_Serializers_TypeModeData mode_Ray1_PC_Fan = new()
        {
            InitContext = x => x.AddSettings(new Ray1Settings(Ray1EngineVersion.PC_Fan)),
            Game = Games.RaymanByHisFans,
        };
        Utility_Serializers_TypeModeData mode_Ray1_PC_60n = new()
        {
            InitContext = x => x.AddSettings(new Ray1Settings(Ray1EngineVersion.PC_Fan)),
            Game = Games.Rayman60Levels,
        };
        Utility_Serializers_TypeModeData mode_OpenSpace_R2 = new()
        {
            Game = Games.Rayman2,
        };
        Utility_Serializers_TypeModeData mode_OpenSpace_RM = new()
        {
            Game = Games.RaymanM,
        };
        Utility_Serializers_TypeModeData mode_OpenSpace_RA = new()
        {
            Game = Games.RaymanArena,
        };
        Utility_Serializers_TypeModeData mode_OpenSpace_R3 = new()
        {
            Game = Games.Rayman3,
        };
        Utility_Serializers_TypeModeData mode_UbiArt_RO = new()
        {
            InitContext = x => x.AddSettings(new UbiArtSettings(EngineVersion.RaymanOrigins, Platform.PC)),
            Game = Games.RaymanOrigins,
            Endian = Endian.Big,
            GetDefaultDir = () => Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins",
        };
        Utility_Serializers_TypeModeData mode_UbiArt_RL = new()
        {
            InitContext = x => x.AddSettings(new UbiArtSettings(EngineVersion.RaymanLegends, Platform.PC)),
            Game = Games.RaymanLegends,
            Endian = Endian.Big,
            GetDefaultDir = () => Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends",
        };
        Utility_Serializers_TypeModeData mode_UbiArt_RJR = new()
        {
            InitContext = x => x.AddSettings(new UbiArtSettings(EngineVersion.RaymanJungleRun, Platform.PC)),
            Game = Games.RaymanJungleRun,
            GetDefaultDir = () => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + 
                                  "Packages" + 
                                  Games.RaymanJungleRun.GetManager<GameManager_WinStore>().FullPackageName + 
                                  "LocalState",
        };
        Utility_Serializers_TypeModeData mode_UbiArt_RFR = new()
        {
            InitContext = x => x.AddSettings(new UbiArtSettings(EngineVersion.RaymanFiestaRun, Platform.PC)),
            Game = Games.RaymanFiestaRun,
            GetDefaultDir = () => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + 
                                  "Packages" + 
                                  Games.RaymanFiestaRun.GetManager<GameManager_WinStore>().FullPackageName + 
                                  "LocalState",
        };

        // TODO-UPDATE: Localize & include file ext in all the names
        Types = new ObservableCollection<Utility_Serializers_TypeViewModel>()
        {
            new Serializers_TypeViewModel<PC_SaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R1SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman 1 (PC)"), 
                        mode_Ray1_PC with { Encoder = new PC_SaveEncoder() }),
                }),

            new Serializers_TypeViewModel<PC_ConfigFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R1ConfigHeader)),
                fileExtension: new FileExtension(".cfg"),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman 1 (PC)"), mode_Ray1_PC),
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman Educational (PC)"), mode_Ray1_PC_Edu),
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman Designer (PC)"), mode_Ray1_PC_Kit),
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman by his Fans (PC)"), mode_Ray1_PC_Fan),
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman 60 Levels (PC)"), mode_Ray1_PC_60n),
                }),

            new Serializers_TypeViewModel<R2GeneralSaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R2SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman 2 (PC)"), 
                        mode_OpenSpace_R2 with { Encoder = new PC_SaveEncoder() }),
                }),

            new Serializers_TypeViewModel<R2ConfigFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R2ConfigHeader)),
                fileExtension: new FileExtension(".cfg"),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman 2 (PC)"), 
                        mode_OpenSpace_R2 with { Encoder = new PC_SaveEncoder() }),
                }),

            new Serializers_TypeViewModel<RMSaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RMSaveHeader)),
                fileExtension: new FileExtension(".sav"),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman M (PC)"), mode_OpenSpace_RM),
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman Arena (PC)"), mode_OpenSpace_RA),
                }),

            new Serializers_TypeViewModel<R3SaveFile>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_R3SaveHeader)),
                fileExtension: new FileExtension(".sav"),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman 3 (PC)"), 
                        mode_OpenSpace_R3 with { Encoder = new R3SaveEncoder() }),
                }),

            new Serializers_TypeViewModel<Origins_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_ROSaveHeader)),
                fileExtension: new FileExtension(""),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman Origins (PC)"), mode_UbiArt_RO),
                }),

            new Serializers_TypeViewModel<Legends_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RLSaveHeader)),
                fileExtension: new FileExtension(""),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman Legends (PC)"), mode_UbiArt_RL),
                }),

            new Serializers_TypeViewModel<JungleRun_SaveData>(
                name: new ResourceLocString(nameof(Resources.Utilities_Converter_RJRSaveHeader)),
                fileExtension: new FileExtension(".dat"),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman Jungle Run (PC/Android/iOS)"), mode_UbiArt_RJR),
                }),

            new Serializers_TypeViewModel<FiestaRun_SaveData>(
                name: new ConstLocString("Rayman Fiesta Run Save File"),
                fileExtension: new FileExtension(".dat"),
                modes: new ObservableCollection<Utility_Serializers_TypeModeViewModel>()
                {
                    new Utility_Serializers_TypeModeViewModel(new ConstLocString("Rayman Fiesta Run (PC/Android/iOS)"), mode_UbiArt_RFR),
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
            FileSystemPath? defaultDir = SelectedType.SelectedMode.Data.GetDefaultDir();

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
                    SerializerLog log = new();

                    using RCPContext context = new(fileResult.SelectedFiles.First().Parent, log: log);
                    SelectedType.SelectedMode.Data.InitContext(context);

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
            FileSystemPath? defaultDir = SelectedType.SelectedMode.Data.GetDefaultDir();

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
                    SelectedType.SelectedMode.Data.InitContext(context);

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
                    SelectedType.SelectedMode.Data.InitContext(context);

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

    #region Classes

    private class SerializerLog : ISerializerLog
    {
        public bool IsEnabled => true;

        private StringBuilder? _stringBuilder = new();

        public string? GetString => _stringBuilder?.ToString();

        public void Log(object? obj)
        {
            _stringBuilder?.AppendLine(obj != null ? obj.ToString() : String.Empty);
        }

        public void Dispose()
        {
            _stringBuilder = null;
        }
    }

    #endregion
}