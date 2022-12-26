﻿using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

[BaseGameComponent]
[SingleInstanceGameComponent]
public class Win32LaunchArgsComponent : FactoryGameComponent<string?>
{
    public Win32LaunchArgsComponent(Func<GameInstallation, string?> objFactory) : base(objFactory) { }
}