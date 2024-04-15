using RayCarrot.RCP.Metro.ModLoader.Modules;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
[GameFeature(nameof(Resources.ModLoader_Title), GenericIconKind.GameFeature_ModLoader)]
public class ModModuleComponent : FactoryGameComponent<ModModule>
{
    public ModModuleComponent(Func<GameInstallation, ModModule> objFactory) : base(objFactory) { }

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<GamePanelComponent, ModLoaderGamePanelComponent>();
    }
}