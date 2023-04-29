using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro.Games.Components;

public class Rayman3DemoCPATextureSyncComponent : CPATextureSyncComponent
{
    public Rayman3DemoCPATextureSyncComponent() : base(
        new CPATextureSyncDataItem(
            Name: "Gamedatabin",
            Archives: new[] { "tex32.cnt", "vignette.cnt" })) 
    { }
}