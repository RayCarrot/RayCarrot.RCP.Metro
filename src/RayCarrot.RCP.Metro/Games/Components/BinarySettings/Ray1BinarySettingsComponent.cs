using BinarySerializer;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Games.Components;

public class Ray1BinarySettingsComponent : BinarySettingsComponent
{
    public Ray1BinarySettingsComponent(Ray1Settings settings)
    {
        Settings = settings;
    }

    public Ray1Settings Settings { get; }

    public override void AddSettings(Context context)
    {
        context.AddSettings(Settings);
    }
}