namespace RayCarrot.RCP.Metro.Games.Components;

public class RemoveFromJumpListOnGameRemovedComponent : OnGameRemovedComponent
{
    public RemoveFromJumpListOnGameRemovedComponent() : base(RemoveFromJumpList) { }

    private static void RemoveFromJumpList(GameInstallation gameInstallation)
    {
        Services.JumpList.RemoveGame(gameInstallation);
    }
}