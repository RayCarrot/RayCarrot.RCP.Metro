﻿using System.IO;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.Settings;

public class Rayman2SettingsViewModel : GameSettingsViewModel
{
    #region Constructor

    public Rayman2SettingsViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        FileSystemPath appDataFilePath = GetAppDataFilePath();
        Logger.Info("The app data path has been retrieved as {0}", appDataFilePath);

        AppDataManager = new UbisoftIniAppDataManager<Rayman2IniAppData>(appDataFilePath);

        GraphicsMode = new GraphicsModeSelectionViewModel();
        GraphicsMode.GraphicsModeChanged += (_, _) => UnsavedChanges = true;

        CanModifyGameFiles = Services.File.CheckDirectoryWriteAccess(GameInstallation.InstallLocation.Directory);

        if (!CanModifyGameFiles)
            Logger.Info("The game files for {0} can't be modified", GameInstallation.FullId);

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

    public UbisoftIniAppDataManager<Rayman2IniAppData> AppDataManager { get; }
    public GraphicsModeSelectionViewModel GraphicsMode { get; }
    public bool CanModifyGameFiles { get; }

    public ObservableCollection<ButtonMapperKeyItemViewModel> Keys { get; }

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

    #endregion

    #region Protected Methods

    protected override async Task LoadAsync()
    {
        AddSettingsLocation(LinkItemViewModel.LinkType.File, AppDataManager.AppDataFilePath);

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

            if (dinputType == DinputType.ButtonMapping)
            {
                HashSet<Rayman2ButtonMappingItem>? result = null;

                Logger.Info("Loading current button mapping...");

                try
                {
                    // Load current mapping
                    result = await Rayman2ButtonMappingManager.LoadCurrentMappingAsync(GetDinputFilePath());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Loading current Rayman 2 button mapping");
                }

                if (result != null)
                {
                    Logger.Debug("{0} key items retrieved", result.Count);

                    // Process each item
                    foreach (Rayman2ButtonMappingItem item in result)
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

        UnsavedChanges = false;
    }

    protected override async Task SaveAsync()
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

        if (CanModifyGameFiles)
        {
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
                    List<Rayman2ButtonMappingItem> items = Keys.
                        // Get only the ones that have changed
                        Where(x => x.NewKey != x.OriginalKey).
                        // Convert to a key mapping item
                        Select(x => new Rayman2ButtonMappingItem(DirectXKeyHelpers.GetKeyCode(x.OriginalKey),
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
                        await Rayman2ButtonMappingManager.ApplyMappingAsync(dinputFilePath, items);
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
    }

    protected override void SettingsPropertyChanged(string propertyName)
    {
        if (propertyName is
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