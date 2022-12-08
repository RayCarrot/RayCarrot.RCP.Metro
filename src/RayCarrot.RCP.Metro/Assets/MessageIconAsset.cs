namespace RayCarrot.RCP.Metro;

[AssetDirectory($"{Assets.AssetsPath}/MessageIcons")]
public enum MessageIconAsset
{
    [AssetFileName("Error.png")]
    Error,

    [AssetFileName("Generic.png")]
    Generic,

    [AssetFileName("Happy.png")]
    Happy,

    [AssetFileName("Info.png")]
    Info,

    [AssetFileName("Question.png")]
    Question,
}