﻿namespace RayCarrot.RCP.Metro;

[AssetDirectory($"{Assets.AssetsPath}/GamePlatformIcons")]
public enum GamePlatformIconAsset
{
    [AssetFileName("MSDOS.png")]
    MsDos,

    [AssetFileName("Win32.png")]
    Win32,

    [AssetFileName("WindowsPackage.png")]
    WindowsPackage,

    [AssetFileName("Gbc.png")]
    Gbc,

    [AssetFileName("PlayStation.png")]
    Ps1,

    [AssetFileName("Gba.png")]
    Gba,
}