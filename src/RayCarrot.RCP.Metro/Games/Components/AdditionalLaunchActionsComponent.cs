﻿using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// Game component for providing additional launch actions for an installed game
/// </summary>
public class AdditionalLaunchActionsComponent : FactoryGameComponent<IEnumerable<ActionItemViewModel>>
{
    public AdditionalLaunchActionsComponent(Func<GameInstallation, IEnumerable<ActionItemViewModel>> objFactory) : base(objFactory) 
    { }
}