namespace RayCarrot.RCP.Metro.Games.Components;

// NOTE: Ideally we'd handle this a bit better. The only reason we need it is as the dialogs might access
//       the same ubi.ini file.
[GameComponentBase]
public class GameOptionsDialogGroupNameComponent : GameComponent
{
    public GameOptionsDialogGroupNameComponent(string groupName)
    {
        GroupName = groupName;
    }

    public string GroupName { get; }
}