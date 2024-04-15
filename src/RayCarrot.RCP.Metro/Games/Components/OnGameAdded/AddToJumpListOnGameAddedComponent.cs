namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentInstance(DefaultPriority = ComponentPriority.Low)]
public class AddToJumpListOnGameAddedComponent : OnGameAddedComponent
{
    public AddToJumpListOnGameAddedComponent() : base(AddToJumpList) { }

    private static void AddToJumpList(GameInstallation gameInstallation)
    {
        Services.JumpList.AddGame(gameInstallation);
    }
}