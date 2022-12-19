namespace RayCarrot.RCP.Metro;

public class RemoveFromJumpListOnGameRemovedComponent : OnGameRemovedComponent
{
    public RemoveFromJumpListOnGameRemovedComponent() : base(RemoveFromJumpList) { }

    private static void RemoveFromJumpList(GameInstallation gameInstallation)
    {
        Services.JumpList.RemoveGame(gameInstallation);
    }
}