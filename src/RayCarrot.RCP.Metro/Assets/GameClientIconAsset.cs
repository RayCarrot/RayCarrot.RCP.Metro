namespace RayCarrot.RCP.Metro;

[AssetDirectory($"{Assets.AssetsPath}/GameClientIcons")]
public enum GameClientIconAsset
{
    [AssetFileName("Custom.png")]
    Custom,

    [AssetFileName("DOSBox.png")]
    DosBox,

    [AssetFileName("mGBA.png")]
    MGBA,

    [AssetFileName("Visual Boy Advance - M.png")]
    VisualBoyAdvanceM,

    [AssetFileName("Steam.png")]
    Steam,

    [AssetFileName("Ubisoft Connect.png")]
    UbisoftConnect,
}