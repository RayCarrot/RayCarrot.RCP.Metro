using System.IO;
using System.Windows.Input;
using BinarySerializer;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class Rayman2ConfigViewModel : ConfigPageViewModel
{
    #region Constructor

    public Rayman2ConfigViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        FileSystemPath appDataFilePath = GetAppDataFilePath();
        Logger.Info("The app data path has been retrieved as {0}", appDataFilePath);

        AppDataManager = new UbisoftIniAppDataManager<Rayman2IniAppData>(appDataFilePath);

        CanModifyGameFiles = Services.File.CheckDirectoryWriteAccess(GameInstallation.InstallLocation.Directory);

        if (!CanModifyGameFiles)
            Logger.Info("The game files for {0} can't be modified", GameInstallation.FullId);

        IsHorizontalWidescreen = true;

        Keys = new ObservableCollection<ButtonMapperKeyItemViewModel>()
        {
            new(new ResourceLocString(nameof(Resources.Config_Action_Up)), Key.Up, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Down)), Key.Down, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Left)), Key.Left, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Right)), Key.Right, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_JumpSwimUp)), Key.A, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Config_Action_SwimDown)), Key.Z, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Shoot)), Key.Space, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_WalkSlow)), Key.LeftShift, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Strafe)), Key.LeftCtrl, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_CamRight)), Key.Q, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_CamLeft)), Key.W, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_CamCenter)), Key.End, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Look)), Key.NumPad0, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Screenshot)), Key.F8, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_HUD)), Key.J, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_R2Guide)), Key.F1, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Confirm)), Key.Enter, keyChanged),
            new(new ResourceLocString(nameof(Resources.Config_Action_Cancel)), Key.Escape, keyChanged)
        };

        void keyChanged() => UnsavedChanges = true;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Constants

    private const string GLI_DllFile_DirectX = "GliDX6";
    private const string GLI_Dll_DirectX = "DirectX6";
    private const string GLI_Driver_DirectX = "display";
    private const string GLI_Device_DirectX = "Direct3D HAL";

    private const string GLI_DllFile_Glide = "GliVd1";
    private const string GLI_Dll_Glide = "Glide2";
    private const string GLI_Driver_Glide = "";
    private const string GLI_Device_Glide = "";

    #endregion

    #region Private Fields

    private R2GraphicsMode _selectedGraphicsMode;

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public UbisoftIniAppDataManager<Rayman2IniAppData> AppDataManager { get; }
    public bool CanModifyGameFiles { get; }

    public ObservableCollection<ButtonMapperKeyItemViewModel> Keys { get; }

    public bool WidescreenSupport { get; set; }
    public bool IsHorizontalWidescreen { get; set; }

    public bool ControllerSupport { get; set; }

    public Rayman2Language CurrentLanguage { get; set; }

    public R2GraphicsMode SelectedGraphicsMode
    {
        get => _selectedGraphicsMode;
        set
        {
            _selectedGraphicsMode = value;

            if (SelectedGraphicsMode == R2GraphicsMode.DirectX)
            {
                GLI_DllFile = GLI_DllFile_DirectX;
                GLI_Dll = GLI_Dll_DirectX;
                GLI_Driver = GLI_Driver_DirectX;
                GLI_Device = GLI_Device_DirectX;
            }
            else if (SelectedGraphicsMode == R2GraphicsMode.Glide)
            {
                GLI_DllFile = GLI_DllFile_Glide;
                GLI_Dll = GLI_Dll_Glide;
                GLI_Driver = GLI_Driver_Glide;
                GLI_Device = GLI_Device_Glide;
            }
        }
    }

    public string GLI_DllFile { get; set; } = String.Empty;
    public string GLI_Dll { get; set; } = String.Empty;
    public string GLI_Driver { get; set; } = String.Empty;
    public string GLI_Device { get; set; } = String.Empty;

    #endregion

    #region Private Methods

    private FileSystemPath GetAppDataFilePath()
    {
        FileSystemPath localPath = GameInstallation.InstallLocation.Directory + "ubi.ini";

        if (localPath.FileExists)
            return localPath;

        FileSystemPath windowsPath = AppFilePaths.UbiIniPath;

        if (windowsPath.FileExists)
            return windowsPath;

        return FileSystemPath.EmptyPath;
    }

    private FileSystemPath GetDinputFilePath()
    {
        return GameInstallation.InstallLocation.Directory + "dinput.dll";
    }

    private int GetAspectRatioExeOffset(FileSystemPath path)
    {
        // Get the size to determine the version
        long length = path.GetSize();

        // Get the byte location
        int location = length switch
        {
            676352 => 633496, // Disc version
            1468928 => 640152, // GOG version
            _ => -1
        };

        return location;
    }

    private DinputType GetDinputType()
    {
        FileSystemPath path = GetDinputFilePath();

        if (!path.FileExists)
            return DinputType.None;

        try
        {
            long size = path.GetSize();

            return size switch
            {
                136704 => DinputType.ButtonMapping,
                66560 => DinputType.Controller,
                _ => DinputType.Unknown
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting Rayman 2 dinput file size");
            return DinputType.Unknown;
        }
    }

    private bool? CheckIsAspectRatioModified()
    {
        try
        {
            // Get the exe file path
            DirectoryProgramInstallationStructure programStructure = GameInstallation.GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>();
            FileSystemPath path = programStructure.FileSystem.GetAbsolutePath(GameInstallation, ProgramPathType.PrimaryExe);

            // Get the offset
            int offset = GetAspectRatioExeOffset(path);

            if (offset == -1)
                return null;

            // Open the file
            using Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using Reader reader = new(stream);

            // Read the value
            stream.Position = offset;
            float value = reader.ReadSingle();

            // Check if the value has been modified
            if (value != 1)
            {
                Logger.Info("The Rayman 2 aspect ratio has been detected as modified");
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if Rayman 2 aspect ratio has been modified");
            return null;
        }
    }

    private async Task SetAspectRatioAsync()
    {
        try
        {
            Logger.Info("The Rayman 2 aspect ratio is being set...");

            // Get the exe file path
            var programStructure = GameInstallation.GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>();
            FileSystemPath path = programStructure.FileSystem.GetAbsolutePath(GameInstallation, ProgramPathType.PrimaryExe);

            // Make sure the file exists
            if (!path.FileExists)
            {
                Logger.Warn("The Rayman 2 executable could not be found");

                if (WidescreenSupport)
                    await Services.MessageUI.DisplayMessageAsync(Resources.R2Widescreen_ExeNotFound, MessageType.Error);

                return;
            }

            // Get the offset
            int offset = GetAspectRatioExeOffset(path);

            Logger.Debug("The aspect ratio value offset has been detected as {0}", offset);

            // Cancel if unknown version
            if (offset == -1)
            {
                Logger.Info("The Rayman 2 executable file size does not match any supported version");

                if (WidescreenSupport)
                    await Services.MessageUI.DisplayMessageAsync(Resources.R2Widescreen_ExeNotValid, MessageType.Error);

                return;
            }

            // Apply widescreen patch
            if (WidescreenSupport)
            {
                // Get the aspect ratio
                float value = IsHorizontalWidescreen ? (float)GraphicsMode.Height / GraphicsMode.Width : (float)GraphicsMode.Width / GraphicsMode.Height;

                // Multiply by 4/3
                value *= 4.0F / 3.0F;

                // Open the file
                using Stream stream = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.Read);
                using Writer writer = new(stream);

                // Write the value
                stream.Position = offset;
                writer.Write(value);

                Logger.Info("The Rayman 2 aspect ratio has been set");
            }
            // Restore to 4/3 if modified previously
            else
            {
                // Open the file for reading
                float value;
                using (Stream readStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using Reader reader = new(readStream);

                    // Read the value
                    readStream.Position = offset;
                    value = reader.ReadSingle();
                }

                // Check if the data has been modified
                if (value != 1)
                {
                    Logger.Info("The Rayman 2 aspect ratio has been detected as modified");

                    using Stream writeStream = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.Read);
                    using Writer writer = new(writeStream);

                    // Write the value
                    writeStream.Position = offset;
                    writer.Write(1f);

                    Logger.Info("The Rayman 2 aspect ratio has been restored");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting Rayman 2 aspect ratio");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R2Widescreen_Error);

            if (WidescreenSupport)
                throw;
        }
    }

    #endregion

    #region Protected Methods

    protected override async Task LoadAsync()
    {
        Logger.Info("{0} config is being set up", GameInstallation.FullId);

        AddConfigLocation(LinkItemViewModel.LinkType.File, AppDataManager.AppDataFilePath);

        await AppDataManager.EnableUbiIniWriteAccessAsync();

        // Load the app data
        AppDataManager.Load();

        GraphicsMode.GetAvailableResolutions();

        if (CpaDisplayMode.TryParse(AppDataManager.AppData.GLI_Mode, out CpaDisplayMode displayMode))
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(displayMode.Width, displayMode.Height);
        else
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(800, 600);

        CurrentLanguage = Enum.TryParse(AppDataManager.AppData.Language, out Rayman2Language lang) ? lang : Rayman2Language.English;

        GLI_DllFile = AppDataManager.AppData.GLI_DllFile;
        GLI_Dll = AppDataManager.AppData.GLI_Dll;
        GLI_Driver = AppDataManager.AppData.GLI_Driver;
        GLI_Device = AppDataManager.AppData.GLI_Device;

        if (GLI_DllFile.Equals(GLI_DllFile_DirectX, StringComparison.InvariantCultureIgnoreCase) &&
            GLI_Dll.Equals(GLI_Dll_DirectX, StringComparison.InvariantCultureIgnoreCase) &&
            GLI_Driver.Equals(GLI_Driver_DirectX, StringComparison.InvariantCultureIgnoreCase) &&
            GLI_Device.Equals(GLI_Device_DirectX, StringComparison.InvariantCultureIgnoreCase))
        {
            _selectedGraphicsMode = R2GraphicsMode.DirectX;
            OnPropertyChanged(nameof(SelectedGraphicsMode));
        }
        else if (GLI_DllFile.Equals(GLI_DllFile_Glide, StringComparison.InvariantCultureIgnoreCase) &&
                 GLI_Dll.Equals(GLI_Dll_Glide, StringComparison.InvariantCultureIgnoreCase) &&
                 GLI_Driver.Equals(GLI_Driver_Glide, StringComparison.InvariantCultureIgnoreCase) &&
                 GLI_Device.Equals(GLI_Device_Glide, StringComparison.InvariantCultureIgnoreCase))
        {
            _selectedGraphicsMode = R2GraphicsMode.Glide;
            OnPropertyChanged(nameof(SelectedGraphicsMode));
        }
        else
        {
            _selectedGraphicsMode = R2GraphicsMode.Custom;
            OnPropertyChanged(nameof(SelectedGraphicsMode));
        }

        if (CanModifyGameFiles)
        {
            // Get the dinput type
            DinputType dinputType = GetDinputType();

            Logger.Info("The dinput type has been retrieved as {0}", dinputType);

            ControllerSupport = dinputType == DinputType.Controller;

            // Check if the aspect ratio has been modified
            if (CheckIsAspectRatioModified() == true)
                WidescreenSupport = true;

            if (dinputType == DinputType.ButtonMapping)
            {
                HashSet<Rayman2ConfigButtonMappingItem>? result = null;

                Logger.Info("Loading current button mapping...");

                try
                {
                    // Load current mapping
                    result = await Rayman2ConfigButtonMappingManager.LoadCurrentMappingAsync(GetDinputFilePath());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Loading current Rayman 2 button mapping");
                }

                if (result != null)
                {
                    Logger.Debug("{0} key items retrieved", result.Count);

                    // Process each item
                    foreach (Rayman2ConfigButtonMappingItem item in result)
                    {
                        // Get the original key
                        Key originalKey = DirectXKeyHelpers.GetKey(item.OriginalKey);

                        // Attempt to get corresponding Rayman 2 key
                        ButtonMapperKeyItemViewModel? r2Item = Keys.FirstOrDefault(x => x.OriginalKey == originalKey);

                        // If one was found, set the new key
                        r2Item?.SetInitialNewKey(DirectXKeyHelpers.GetKey(item.NewKey));
                    }
                }
            }
        }

        Logger.Info("All config properties have been loaded");

        UnsavedChanges = false;
    }

    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("{0} configuration is saving...", GameInstallation.FullId);

        try
        {
            AppDataManager.AppData.GLI_Mode = new CpaDisplayMode()
            {
                BitsPerPixel = 16,
                IsFullscreen = true,
                Width = GraphicsMode.Width,
                Height = GraphicsMode.Height,
            }.ToString();

            AppDataManager.AppData.Language = CurrentLanguage.ToString();
            AppDataManager.AppData.GLI_DllFile = GLI_DllFile;
            AppDataManager.AppData.GLI_Dll = GLI_Dll;
            AppDataManager.AppData.GLI_Driver = GLI_Driver;
            AppDataManager.AppData.GLI_Device = GLI_Device;

            // Save the config data
            AppDataManager.Save();

            Logger.Info("{0} configuration has been saved", GameInstallation.FullId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving ubi.ini data");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, GameInstallation.GetDisplayName()), Resources.Config_SaveErrorHeader);
            return false;
        }

        if (CanModifyGameFiles)
        {
            try
            {
                // Set the aspect ratio
                await SetAspectRatioAsync();

                try
                {
                    // Get the current dinput type
                    DinputType dinputType = GetDinputType();
                    FileSystemPath dinputFilePath = GetDinputFilePath();

                    Logger.Info("The dinput type has been retrieved as {0}", dinputType);

                    if (ControllerSupport)
                    {
                        if (dinputType != DinputType.Controller)
                        {
                            // Attempt to delete existing dinput file
                            if (dinputType != DinputType.None)
                                File.Delete(dinputFilePath);

                            // Write controller patch
                            File.WriteAllBytes(dinputFilePath, Files.dinput_controller);
                        }
                    }
                    else
                    {
                        List<Rayman2ConfigButtonMappingItem> items = Keys.
                            // Get only the ones that have changed
                            Where(x => x.NewKey != x.OriginalKey).
                            // Convert to a key mapping item
                            Select(x => new Rayman2ConfigButtonMappingItem(DirectXKeyHelpers.GetKeyCode(x.OriginalKey),
                                DirectXKeyHelpers.GetKeyCode(x.NewKey))).
                            // Convert to a list
                            ToList();

                        if (items.Any())
                        {
                            if (dinputType != DinputType.ButtonMapping)
                            {
                                // Attempt to delete existing dinput file
                                if (dinputType != DinputType.None)
                                    File.Delete(dinputFilePath);

                                // Write template file
                                File.WriteAllBytes(dinputFilePath, Files.dinput_mapping);
                            }

                            // Apply the patch
                            await Rayman2ConfigButtonMappingManager.ApplyMappingAsync(dinputFilePath, items);
                        }
                        else
                        {
                            // Attempt to delete existing dinput file
                            if (dinputType != DinputType.Unknown && dinputType != DinputType.None)
                                File.Delete(dinputFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Saving Rayman 2 dinput hack data");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Saving game modifications");
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Config_SaveWarning, Resources.Config_SaveErrorHeader);

                return false;
            }
        }

        return true;
    }

    protected override void ConfigPropertyChanged(string propertyName)
    {
        if (propertyName is
            nameof(WidescreenSupport) or
            nameof(ControllerSupport) or
            nameof(CurrentLanguage) or
            nameof(GLI_DllFile) or
            nameof(GLI_Dll) or
            nameof(GLI_Driver) or
            nameof(GLI_Device))
        {
            UnsavedChanges = true;
        }
    }

    #endregion

    #region Public Methods

    public override void Dispose()
    {
        // Dispose base
        base.Dispose();

        Keys.DisposeAll();
    }

    #endregion

    #region Enums

    /// <summary>
    /// The available types of Rayman 2 dinput.dll file
    /// </summary>
    private enum DinputType
    {
        None,
        Controller,
        ButtonMapping,
        Unknown
    }

    /// <summary>
    /// The available pre-set Rayman 2 graphics modes
    /// </summary>
    public enum R2GraphicsMode
    {
        DirectX,
        Glide,
        Custom
    }

    #endregion
}