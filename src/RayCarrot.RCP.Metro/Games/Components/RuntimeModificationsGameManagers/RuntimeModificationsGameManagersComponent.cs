﻿using RayCarrot.RCP.Metro.Games.Panels;
using RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

namespace RayCarrot.RCP.Metro.Games.Components;

// TODO-UPDATE: Add game feature attribute
[BaseGameComponent]
public class RuntimeModificationsGameManagersComponent : FactoryGameComponent<IEnumerable<GameManager>>
{
    public RuntimeModificationsGameManagersComponent(EmulatedPlatform emulatedPlatform, Func<GameInstallation, IEnumerable<GameManager>> objFactory) : base(objFactory)
    {
        EmulatedPlatform = emulatedPlatform;
    }

    public EmulatedPlatform EmulatedPlatform { get; }

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<GamePanelComponent>(new GameToolGamePanelComponent(x => new RuntimeModificationsGamePanelViewModel(x)));
    }
}