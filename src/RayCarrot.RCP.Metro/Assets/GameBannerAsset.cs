namespace RayCarrot.RCP.Metro;

// TODO-14: Add more banner images
[AssetDirectory($"{Assets.AssetsPath}/GameBanners", defaultFileName: "Default.png")]
public enum GameBannerAsset
{
    [AssetFileName("Default.png")] 
    Default,
    
    [AssetFileName("Rayman 1.png")] 
    Rayman1,

    [AssetFileName("Rayman 2.png")] 
    Rayman2,

    [AssetFileName("Rayman M.png")] 
    RaymanM,

    [AssetFileName("Rayman 3.png")] 
    Rayman3,

    [AssetFileName("Rayman Origins.png")] 
    RaymanOrigins,

    [AssetFileName("Rayman Legends.png")] 
    RaymanLegends,

    [AssetFileName("Rayman Jungle Run.png")]
    RaymanJungleRun,

    [AssetFileName("Rayman Fiesta Run.png")]
    RaymanFiestaRun,
}