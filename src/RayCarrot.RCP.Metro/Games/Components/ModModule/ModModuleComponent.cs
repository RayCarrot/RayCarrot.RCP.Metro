﻿using RayCarrot.RCP.Metro.ModLoader.Modules;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
[GameFeature(nameof(Resources.ModLoader_Title), GenericIconKind.GameFeature_ModLoader)]
public class ModModuleComponent : FactoryGameComponent<ModModule>
{
    public ModModuleComponent(Func<GameInstallation, ModModule> objFactory) : base(objFactory) { }
}