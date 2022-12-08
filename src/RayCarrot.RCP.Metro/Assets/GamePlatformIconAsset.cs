﻿namespace RayCarrot.RCP.Metro;

[AssetDirectory($"{Assets.AssetsPath}/GamePlatformIcons")]
public enum GamePlatformIconAsset
{
    [AssetFileName("MSDOS.png")]
    MSDOS,

    [AssetFileName("Win32.png")]
    Win32,

    [AssetFileName("Steam.png")]
    Steam,

    [AssetFileName("WindowsPackage.png")]
    WindowsPackage,
}