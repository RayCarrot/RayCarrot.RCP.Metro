﻿using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro.Games.Components;

public class CPAGameModeComponent : BinaryGameModeComponent
{
    public CPAGameModeComponent(CPAGameMode gameMode) : base(gameMode) { }

    public OpenSpaceSettings GetSettings() => GetRequiredSettings<OpenSpaceSettings>();
}