namespace RayCarrot.RCP.Metro;

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

    [AssetFileName("Rayman Raving Rabbids.png")]
    RaymanRavingRabbids,

    [AssetFileName("Rabbids Go Home.png")]
    RabbidsGoHome,

    [AssetFileName("Rabbids Big Bang.png")]
    RabbidsBigBang,

    [AssetFileName("Tonic Trouble.png")]
    TonicTrouble,

    [AssetFileName("Rayman Activity Center.png")]
    RaymanActivityCenter,

    [AssetFileName("Rayman Dictées.png")]
    RaymanDictées,

    [AssetFileName("Rayman Premiers Clics.png")]
    RaymanPremiersClics,
}