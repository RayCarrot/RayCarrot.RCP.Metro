namespace RayCarrot.RCP.Metro;

// TODO-14: Add more banner images
[AssetDirectory($"{Assets.AssetsPath}/GameBanners", defaultFileName: "Default.png")]
public enum GameBannerAsset
{
    [AssetFileName("Default.png")] 
    Default,
    
    [AssetFileName("Rayman 2.png")] 
    Rayman2,

    [AssetFileName("Rayman Legends.png")] 
    RaymanLegends,
}