namespace RayCarrot.RCP.Metro.Games.Components;

public abstract class GameValidationCheckComponent : DescriptorComponent
{
    public abstract bool IsValid(GameInstallation gameInstallation);
}