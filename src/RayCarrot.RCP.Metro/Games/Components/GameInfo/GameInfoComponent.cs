﻿using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

[GameComponentBase]
public class GameInfoComponent : FactoryGameComponent<IEnumerable<DuoGridItemViewModel>>
{
    public GameInfoComponent(Func<GameInstallation, IEnumerable<DuoGridItemViewModel>> objFactory) : base(objFactory) { }
}