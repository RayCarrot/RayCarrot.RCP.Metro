namespace RayCarrot.RCP.Metro;

[AssetDirectory($"{Assets.AssetsPath}/GameCategoryIcons")]
public enum GameCategoryIconAsset
{
    [AssetFileName("Rayman.png")] 
    Rayman,

    [AssetFileName("Rabbids.png")] 
    Rabbids,

    [AssetFileName("Handheld.png")] 
    Handheld,

    [AssetFileName("Fan.png")] 
    Fan,

    [AssetFileName("Other.png")] 
    Other,
}