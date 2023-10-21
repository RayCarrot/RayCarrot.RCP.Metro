namespace RayCarrot.RCP.Metro;

[AssetDirectory($"{Assets.AssetsPath}/AppNewsIcons", defaultFileName: "Main.png")]
public enum AppNewsIconAsset
{
    [AssetFileName("Main.png")] 
    Main,
    
    [AssetFileName("Rayman.png")] 
    Rayman,

    [AssetFileName("Patcher.png")] 
    Patcher,
}