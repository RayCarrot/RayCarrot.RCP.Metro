using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman 3 configuration
/// </summary>
public class Rayman3ConfigViewModel : BaseRayman3MArenaConfigViewModel<Rayman3IniAppData, Rayman3Language>
{
    #region Constructor

    public Rayman3ConfigViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    #endregion

    #region Protected Properties

    protected override FilePatcher_Patch[]? RemoveDiscCheckPatches => null;

    #endregion

    #region Public Properties

    public override bool HasControllerConfig => true;
    public override bool HasNetworkConfig => false;

    #endregion

    #region Protected Methods

    protected override void LoadAppData()
    {
        if (CpaDisplayMode.TryParse(AppDataManager.AppData.GLI_Mode, out CpaDisplayMode displayMode))
        {
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(displayMode.Width, displayMode.Height);
            FullscreenMode = displayMode.IsFullscreen;
            IsTextures32Bit = displayMode.BitsPerPixel != 16;
        }
        else
        {
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(800, 600);
            FullscreenMode = true;
            IsTextures32Bit = true;
        }

        TriLinear = AppDataManager.AppData.TriLinear != 0;
        TnL = AppDataManager.AppData.TnL != 0;
        CompressedTextures = AppDataManager.AppData.TexturesCompressed != 0;
        VideoQuality = AppDataManager.AppData.Video_WantedQuality;
        AutoVideoQuality = AppDataManager.AppData.Video_AutoAdjustQuality != 0;
        IsVideo32Bpp = AppDataManager.AppData.Video_BPP != 16;
        CurrentLanguage = Enum.TryParse(AppDataManager.AppData.Language, out Rayman3Language lang) ? lang : Rayman3Language.English;
        VerticalAxis = AppDataManager.AppData.Camera_VerticalAxis;
        HorizontalAxis = AppDataManager.AppData.Camera_HorizontalAxis;
    }

    protected override void SaveAppData()
    {
        AppDataManager.AppData.GLI_Mode = new CpaDisplayMode()
        {
            BitsPerPixel = IsTextures32Bit ? 32 : 16,
            IsFullscreen = FullscreenMode,
            Width = GraphicsMode.Width,
            Height = GraphicsMode.Height,
        }.ToString();

        AppDataManager.AppData.TriLinear = TriLinear ? 1 : 0;
        AppDataManager.AppData.TnL = TnL ? 1 : 0;
        AppDataManager.AppData.TexturesCompressed = CompressedTextures ? 1 : 0;
        AppDataManager.AppData.Video_WantedQuality = VideoQuality;
        AppDataManager.AppData.Video_AutoAdjustQuality = AutoVideoQuality ? 1 : 0;
        AppDataManager.AppData.Video_BPP = IsVideo32Bpp ? 32 : 16;
        AppDataManager.AppData.Language = CurrentLanguage.ToString();
        AppDataManager.AppData.Camera_VerticalAxis = VerticalAxis;
        AppDataManager.AppData.Camera_HorizontalAxis = HorizontalAxis;
        AppDataManager.AppData.TexturesFile = $"Tex{(IsTextures32Bit ? 32 : 16)}.cnt";
    }

    #endregion
}