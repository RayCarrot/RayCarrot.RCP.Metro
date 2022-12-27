﻿using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class GameConfigComponent : FactoryGameComponent<ConfigPageViewModel>
{
    public GameConfigComponent(Func<GameInstallation, ConfigPageViewModel> objFactory) : base(objFactory) { }
}