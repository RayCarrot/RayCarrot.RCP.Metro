using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
public class CPATextureSyncComponent : GameComponent
{
    public CPATextureSyncComponent(params CPATextureSyncDataItem[] textureSyncItems)
    {
        TextureSyncItems = textureSyncItems;
    }

    public CPATextureSyncDataItem[] TextureSyncItems { get; }
}