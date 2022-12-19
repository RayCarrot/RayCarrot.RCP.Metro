namespace RayCarrot.RCP.Metro;

public class AddToJumpListOnGameAddedComponent : OnGameAddedComponent
{
    public AddToJumpListOnGameAddedComponent() : base(AddToJumpList) { }

    private static void AddToJumpList(GameInstallation gameInstallation)
    {
        Services.JumpList.AddGame(gameInstallation);
    }
}