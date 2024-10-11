using System.IO;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Library;

namespace RayCarrot.RCP.Metro.Games.Settings;

public class UbiArtCommandArgsViewModel : BaseViewModel
{
    public UbiArtCommandArgsViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        Settings = GameInstallation.GetRequiredComponent<BinaryGameModeComponent, UbiArtGameModeComponent>().GetSettings();
        CommandArgs = new UbiArtCommandArgs();
        EditorFields = new ObservableCollectionEx<EditorFieldViewModel>();
        EditorFields.EnableCollectionSynchronization();

        UpdateArgsFromTextCommand = new RelayCommand(UpdateArgsFromText);
        UpdateTextFromArgsCommand = new RelayCommand(UpdateTextFromArgs);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private const string FileName = "cmdline.txt";

    public ICommand UpdateArgsFromTextCommand { get; }
    public ICommand UpdateTextFromArgsCommand { get; }

    public UbiArtCommandArgs CommandArgs { get; }

    public GameInstallation GameInstallation { get; }
    public UbiArtSettings Settings { get; }
    public ObservableCollectionEx<EditorFieldViewModel> EditorFields { get; }
    
    public UbiArtCommandArgsSource Source { get; set; }
    public SourceAvailabilityViewModel? CommandLineFileSourceAvailability { get; set; }
    public SourceAvailabilityViewModel? LaunchArgumentsSourceAvailability { get; set; }

    public string Text { get; set; } = String.Empty;

    private async Task<IEnumerable<string>> TryGetScenePathsAsync()
    {
        try
        {
            ArchiveComponent archiveComponent = GameInstallation.GetRequiredComponent<ArchiveComponent>();
            UbiArtSettings settings = GameInstallation.GetRequiredComponent<BinaryGameModeComponent, UbiArtGameModeComponent>().GetSettings();
            using Context context = new RCPContext(GameInstallation.InstallLocation.Directory, new RCPSerializerSettings()
            {
                DefaultEndianness = settings.Endian
            });
            context.Initialize(GameInstallation);

            string root = settings.Game == BinarySerializer.UbiArt.Game.RaymanOrigins
                ? $"itf_cooked/{settings.PlatformString.ToLowerInvariant()}/"
                : $"cache/itf_cooked/{settings.PlatformString.ToLowerInvariant()}/";

            List<string> scenePaths = new();

            foreach (string archiveFilePath in archiveComponent.GetArchiveFilePaths())
            {
                BundleFile bundle = await context.ReadRequiredFileDataAsync<BundleFile>(archiveFilePath);

                foreach (BundleFile_FileEntry fileEntry in bundle.FilePack.Files)
                {
                    string fileName = fileEntry.Path.FileName;
                    if (fileName.EndsWith(".isc.ckd") &&
                        !fileName.Contains("graph") &&
                        !fileName.Contains("subscene") &&
                        !fileName.Contains("brick") &&
                        !fileName.Contains("actor"))
                    {
                        // Remove the root path
                        string relativePath = fileEntry.Path.FullPath.Substring(root.Length);
                        
                        // Remove the .ckd extension
                        relativePath = relativePath.Substring(0, relativePath.Length - 4);

                        // Replace forward-slash with back-slash so that it works when passing in as a Steam run URI
                        relativePath = relativePath.Replace('/', '\\');
                        
                        // Add to list
                        scenePaths.Add(relativePath);
                    }
                }
            }

            scenePaths.Sort();

            return scenePaths;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting available scene paths");
            return Enumerable.Empty<string>();
        }
    }

    private EditorFieldViewModel CreateBoolField(LocalizedString header, LocalizedString? info, string name)
    {
        return new EditorBoolFieldViewModel(
            header: header,
            info: info,
            getValueAction: () => CommandArgs.GetBool(name),
            setValueAction: x =>
            {
                CommandArgs.SetBool(name, x);
                UpdateTextFromArgs();
            });
    }

    private EditorFieldViewModel CreateIntField(LocalizedString header, LocalizedString? info, string name)
    {
        return new EditorStringFieldViewModel(
            header: header,
            info: info,
            getValueAction: () => CommandArgs.GetInt(name).ToString(),
            setValueAction: x =>
            {
                CommandArgs.SetInt(name, !x.IsNullOrWhiteSpace() && Int32.TryParse(x, out int parsedInt) ? parsedInt : null);
                UpdateTextFromArgs();
            });
    }

    private EditorFieldViewModel CreateStringField(LocalizedString header, LocalizedString? info, string name, IEnumerable<string>? options = null)
    {
        if (options == null)
            return new EditorStringFieldViewModel(
                header: header,
                info: info,
                getValueAction: () => CommandArgs.GetString(name),
                setValueAction: x =>
                {
                    CommandArgs.SetString(name, x);
                    UpdateTextFromArgs();
                });
        else
            return new EditorStringWithOptionsFieldViewModel(
                header: header,
                info: info,
                getValueAction: () => CommandArgs.GetString(name),
                setValueAction: x =>
                {
                    CommandArgs.SetString(name, x);
                    UpdateTextFromArgs();
                },
                options: options);
    }

    private EditorFieldViewModel CreateIntDropDownField(LocalizedString header, LocalizedString? info, string name, IEnumerable<EditorDropDownFieldViewModel.DropDownItem> items)
    {
        return new EditorDropDownFieldViewModel(
            header: header,
            info: info,
            getValueAction: () => (CommandArgs.GetInt(name) ?? -1) + 1,
            setValueAction: x =>
            {
                x--;
                CommandArgs.SetInt(name, x == -1 ? null : x);
                UpdateTextFromArgs();
            },
            getItemsAction: () => items.Prepend(new EditorDropDownFieldViewModel.DropDownItem(new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_DefaultOption)), null)).ToList());
    }

    private async Task CreateEditorFieldsAsync()
    {
        IEnumerable<string> scenePaths = await TryGetScenePathsAsync();

        EditorFields.ModifyCollection(editorFields =>
        {
            editorFields.Clear();

            if (Settings.Game is BinarySerializer.UbiArt.Game.RaymanOrigins or BinarySerializer.UbiArt.Game.RaymanLegends)
            {
                // root
                editorFields.Add(CreateStringField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_RootHeader)),
                    info: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_RootInfo)),
                    name: "root"));

                // map
                editorFields.Add(CreateStringField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_MapHeader)),
                    info: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_MapInfo)),
                    name: "map",
                    options: scenePaths));

                // language
                editorFields.Add(CreateIntDropDownField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_LanguageHeader)),
                    info: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_LanguageInfo)),
                    name: "language",
                    items: Settings.Game switch
                    {
                        // TODO: Localize language names?
                        BinarySerializer.UbiArt.Game.RaymanOrigins => new EditorDropDownFieldViewModel.DropDownItem[]
                        {
                            new("English", null),               // 0
                            new("French", null),                // 1
                            new("Japanese", null),              // 2
                            new("German", null),                // 3
                            new("Spanish", null),               // 4
                            new("Italian", null),               // 5
                            new("Korean", null),                // 6
                            new("Chinese (Traditional)", null), // 7
                            new("Portuguese", null),            // 8
                            new("Chinese (Simplified)", null),  // 9
                            new("Polish", null),                // 10
                            new("Russian", null),               // 11
                            new("Dutch", null),                 // 12
                            new("Czech", null),                 // 13
                            new("Hungarian", null),             // 14
                        },
                        BinarySerializer.UbiArt.Game.RaymanLegends => new EditorDropDownFieldViewModel.DropDownItem[]
                        {
                            new("English", null),               // 0
                            new("French", null),                // 1
                            new("Japanese", null),              // 2
                            new("German", null),                // 3
                            new("Spanish", null),               // 4
                            new("Italian", null),               // 5
                            new("Korean", null),                // 6
                            new("Chinese (Traditional)", null), // 7
                            new("Portuguese", null),            // 8
                            new("Chinese (Simplified)", null),  // 9
                            new("Polish", null),                // 10
                            new("Russian", null),               // 11
                            new("Dutch", null),                 // 12
                            new("Danish", null),                // 13
                            new("Norwegian", null),             // 14
                            new("Swedish", null),               // 15
                            new("Finnish", null),               // 16
                            new("Portuguese (Brazil)", null),   // 17
                            new("Malay", null),                 // 18
                            new("Indonesian", null),            // 19
                            new("Turkish", null),               // 20
                            new("Arabic", null),                // 21
                            new("Tamil", null),                 // 22
                            new("Thai", null),                  // 23
                        },
                        _ => throw new InvalidOperationException("The game is not supported")
                    }));
            }

            if (Settings.Game is BinarySerializer.UbiArt.Game.RaymanLegends)
            {
                // nosave
                editorFields.Add(CreateBoolField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_NoSaveHeader)),
                    info: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_NoSaveInfo)),
                    name: "nosave"));
            }

            if (Settings.Game is BinarySerializer.UbiArt.Game.RaymanOrigins)
            {
                // fps
                editorFields.Add(CreateIntField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_FramerateHeader)),
                    info: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_FramerateInfo)),
                    name: "fps"));

                // nosound
                editorFields.Add(CreateBoolField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_NoSoundHeader)),
                    info: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_NoSoundInfo)),
                    name: "nosound"));

                // camera_maxdezoom
                editorFields.Add(CreateBoolField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_CameraMaxZoomHeader)),
                    info: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_CameraMaxZoomInfo)),
                    name: "camera_maxdezoom"));
            }

            if (Settings.Game is BinarySerializer.UbiArt.Game.RaymanOrigins or BinarySerializer.UbiArt.Game.RaymanLegends)
            {
                // cheatAllPlayersTogether
                editorFields.Add(CreateBoolField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_AllPlayersTogetherHeader)),
                    info: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_AllPlayersTogetherInfo)),
                    name: "cheatAllPlayersTogether"));

                // player_nodamage
                editorFields.Add(CreateBoolField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_NoDamageHeader)),
                    info: null,
                    name: "player_nodamage"));

                // nomouse
                editorFields.Add(CreateBoolField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_NoMouseHeader)),
                    info: null,
                    name: "nomouse"));

                // fullscreen
                editorFields.Add(CreateBoolField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_FullscreenHeader)),
                    info: null,
                    name: "fullscreen"));

                // width
                editorFields.Add(CreateIntField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_WidthHeader)),
                    info: null,
                    name: "width"));

                // height
                editorFields.Add(CreateIntField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_HeightHeader)),
                    info: null,
                    name: "height"));

                // x
                editorFields.Add(CreateIntField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_XHeader)),
                    info: null,
                    name: "x"));

                // y
                editorFields.Add(CreateIntField(
                    header: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_YHeader)),
                    info: null,
                    name: "y"));
            }
        });
    }

    private void RefreshEditorFields()
    {
        foreach (EditorFieldViewModel editorField in EditorFields)
            editorField.Refresh();
    }

    public void UpdateArgsFromText()
    {
        string args = Source switch
        {
            UbiArtCommandArgsSource.CommandLineFile => UbiArtCommandArgs.ReadArgsFromCommandLineFile(Text),
            UbiArtCommandArgsSource.LaunchArguments => Text,
            _ => String.Empty
        };
        CommandArgs.Process(args);
        RefreshEditorFields();
    }

    public void UpdateTextFromArgs()
    {
        Text = Source switch
        {
            UbiArtCommandArgsSource.CommandLineFile => CommandArgs.ToCommandLineFileString(),
            UbiArtCommandArgsSource.LaunchArguments => CommandArgs.ToArgString(),
            _ => String.Empty,
        };
    }

    public async Task LoadAsync()
    {
        if (Settings.Game == BinarySerializer.UbiArt.Game.RaymanOrigins)
        {
            CommandLineFileSourceAvailability = new SourceAvailabilityViewModel(GameInstallation, true);
            LaunchArgumentsSourceAvailability = new SourceAvailabilityViewModel(GameInstallation, false);
        }
        else if (Settings.Game == BinarySerializer.UbiArt.Game.RaymanLegends)
        {
            string[] commandLineModIds =
            {
                "RaymanLegends.CmdLine.Steam",
                "RaymanLegends.CmdLine.Uplay",
            };

            bool installedCommandLineMod = false;
            try
            {
                ModLibrary library = new(GameInstallation);
                ModManifest modManifest = library.ReadModManifest();

                foreach (string modId in commandLineModIds)
                {
                    if (modManifest.Mods.TryGetValue(modId, out ModManifestEntry entry) && entry.IsEnabled)
                    {
                        installedCommandLineMod = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Checking if commandline mod is installed");
            }

            CommandLineFileSourceAvailability = new SourceAvailabilityViewModel(
                gameInstallation: GameInstallation, 
                isAvailable: installedCommandLineMod,
                notAvailableInfo: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_CommandLineFileNotAvailableInfo)),
                requiredGameBananaModId: 0); // TODO-UPDATE: Specify mod id

            LaunchGameComponent? launchGameComponent = GameInstallation.GetComponent<LaunchGameComponent>();
            LaunchArgumentsSourceAvailability = new SourceAvailabilityViewModel(
                gameInstallation: GameInstallation,
                isAvailable: launchGameComponent is { SupportsLaunchArguments: true },
                notAvailableInfo: new ResourceLocString(nameof(Resources.GameSettings_UbiArt_LaunchCommands_LaunchArgumentsNotAvailableInfo)));
        }
        else
        {
            CommandLineFileSourceAvailability = new SourceAvailabilityViewModel(GameInstallation, false);
            LaunchArgumentsSourceAvailability = new SourceAvailabilityViewModel(GameInstallation, false);
        }

        string? args = null;

        // Attempt to read from file
        if (CommandLineFileSourceAvailability.IsAvailable)
        {
            // Get the file path
            FileSystemPath filePath = GameInstallation.InstallLocation.Directory + FileName;

            if (filePath.FileExists)
            {
                try
                {
                    string fileText = File.ReadAllText(filePath);
                    args = UbiArtCommandArgs.ReadArgsFromCommandLineFile(fileText);
                    
                    Text = fileText;
                    Source = UbiArtCommandArgsSource.CommandLineFile;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Reading commandline text file");
                }
            }
        }

        // Use saved args
        if (args == null && LaunchArgumentsSourceAvailability.IsAvailable)
        {
            args = GameInstallation.GetValue<string>(GameDataKey.UbiArt_CommandArgs);

            if (args != null)
            {
                Text = args;
                Source = UbiArtCommandArgsSource.LaunchArguments;
            }
        }

        // Default to empty if no args were found
        args ??= String.Empty;

        // Process the args
        CommandArgs.Process(args);

        // Create the editor fields
        await CreateEditorFieldsAsync();

        // Refresh the editor fields
        RefreshEditorFields();
    }

    public void Save()
    {
        if (Text == String.Empty)
            Source = UbiArtCommandArgsSource.None;

        FileSystemPath commandLineFilePath = GameInstallation.InstallLocation.Directory + FileName;
        if (Source == UbiArtCommandArgsSource.CommandLineFile)
        {
            File.WriteAllText(commandLineFilePath, Text);
        }
        else
        {
            if (commandLineFilePath.FileExists)
                File.Delete(commandLineFilePath);
        }

        if (Source == UbiArtCommandArgsSource.LaunchArguments)
            GameInstallation.SetValue(GameDataKey.UbiArt_CommandArgs, Text);
        else
            GameInstallation.SetValue<string?>(GameDataKey.UbiArt_CommandArgs, null);
    }

    public class SourceAvailabilityViewModel : BaseViewModel
    {
        public SourceAvailabilityViewModel(GameInstallation gameInstallation, bool isAvailable, LocalizedString? notAvailableInfo = null, long? requiredGameBananaModId = null)
        {
            GameInstallation = gameInstallation;
            IsAvailable = isAvailable;
            NotAvailableInfo = notAvailableInfo;
            RequiredGameBananaModId = requiredGameBananaModId;
            RequiresMod = requiredGameBananaModId != null;

            InstallModCommand = new AsyncRelayCommand(InstallModAsync);
        }

        public ICommand InstallModCommand { get; }

        public GameInstallation GameInstallation { get; }
        public bool IsAvailable { get; }

        public LocalizedString? NotAvailableInfo { get; }
        public long? RequiredGameBananaModId { get; }
        public bool RequiresMod { get; }

        public async Task InstallModAsync()
        {
            await Services.UI.ShowModLoaderAsync(GameInstallation, RequiredGameBananaModId ?? throw new Exception("No mod id specified"));
        }
    }
}