using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for an MS-DOS program
/// </summary>
public abstract class MsDosGameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.MsDos;
    public override bool DefaultToUseGameClient => true;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<DosBoxLaunchCommandsComponent, DefaultDosBoxLaunchCommandsComponent>();
    }

    #endregion
}