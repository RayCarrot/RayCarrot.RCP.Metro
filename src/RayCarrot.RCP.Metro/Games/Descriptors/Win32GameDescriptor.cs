using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Win32 game
/// </summary>
public abstract class Win32GameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Win32;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent, Win32LaunchGameComponent>();

        // Default to launch the primary exe file (games may override this by registering another component)
        builder.Register<Win32LaunchPathComponent, DefaultWin32LaunchPathComponent>();
    }

    #endregion
}