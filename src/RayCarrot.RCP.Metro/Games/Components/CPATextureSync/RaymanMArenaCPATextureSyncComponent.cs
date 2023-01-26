using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro.Games.Components;

public class RaymanMArenaCPATextureSyncComponent : CPATextureSyncComponent
{
    public RaymanMArenaCPATextureSyncComponent() : base(
        new CPATextureSyncDataItem(
            Name: "MenuBin",
            Archives: new[] { "tex32.cnt", "vignette.cnt" }),
        new CPATextureSyncDataItem(
            Name: "FishBin",
            Archives: new[] { "tex32.cnt", "vignette.cnt" }),
        new CPATextureSyncDataItem(
            Name: "TribeBin",
            Archives: new[] { "tex32.cnt", "vignette.cnt" })) 
    { }
}