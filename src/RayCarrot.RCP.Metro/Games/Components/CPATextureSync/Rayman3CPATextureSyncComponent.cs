using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro.Games.Components;

public class Rayman3CPATextureSyncComponent : CPATextureSyncComponent
{
    public Rayman3CPATextureSyncComponent() : base(
        new CPATextureSyncDataItem(
            Name: "Gamedatabin",
            Archives: new[] { "tex32_1.cnt", "tex32_2.cnt", "vignette.cnt" })) 
    { }
}