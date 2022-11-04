using System;

namespace RayCarrot.RCP.Metro;

public record CPATextureSyncData(CPAGameMode GameMode, params CPATextureSyncDataItem[] Items)
{
    // TODO-14: Add Donald Duck - seems same as R2?
    public static CPAGameMode[] SupportedGameModes => new[]
    {
        CPAGameMode.TonicTrouble_PC,
        CPAGameMode.TonicTrouble_SE_PC,
        CPAGameMode.Rayman2_PC,
        CPAGameMode.RaymanM_PC,
        CPAGameMode.RaymanArena_PC,
        CPAGameMode.Rayman3_PC
    };

    public static CPATextureSyncData FromGameMode(CPAGameMode gameMode)
    {
        switch (gameMode)
        {
            case CPAGameMode.TonicTrouble_PC:
            case CPAGameMode.TonicTrouble_SE_PC:
                return new CPATextureSyncData(gameMode,
                    new CPATextureSyncDataItem(
                        Name: "GameData",
                        Archives: new[] { "Textures.cnt", "Vignette.cnt" }));

            case CPAGameMode.Rayman2_PC:
                return new CPATextureSyncData(gameMode,
                    new CPATextureSyncDataItem(
                        Name: "Data",
                        Archives: new[] { "Textures.cnt", "Vignette.cnt" }));

            case CPAGameMode.RaymanM_PC:
            case CPAGameMode.RaymanArena_PC:
                return new CPATextureSyncData(gameMode,
                    new CPATextureSyncDataItem(
                        Name: "MenuBin",
                        Archives: new[] { "tex32.cnt", "vignette.cnt" }),
                    new CPATextureSyncDataItem(
                        Name: "FishBin",
                        Archives: new[] { "tex32.cnt", "vignette.cnt" }),
                    new CPATextureSyncDataItem(
                        Name: "TribeBin",
                        Archives: new[] { "tex32.cnt", "vignette.cnt" }));

            case CPAGameMode.Rayman3_PC:
                return new CPATextureSyncData(gameMode,
                    new CPATextureSyncDataItem(
                        Name: "Gamedatabin",
                        Archives: new[] { "tex32_1.cnt", "tex32_2.cnt", "vignette.cnt" }));

            default:
                throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, null);
        }
    }
}