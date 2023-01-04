using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public abstract class BinarySettingsComponent : GameComponent
{
    public abstract void AddSettings(Context context);
}