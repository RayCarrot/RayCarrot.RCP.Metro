namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Fiesta Run save fix utility
/// </summary>
public class Utility_RaymanFiestaRun_SaveFix : Utility<Utility_RaymanFiestaRun_SaveFix_UI, Utility_RaymanFiestaRun_SaveFix_ViewModel>
{
    public override string DisplayHeader => "Fix save progress"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanFiestaRun_SaveFix;
    public override string InfoText => "Due to a bug in earlier versions of the game the save file progress might get out of sync causing you to have fewer teensies than actually earned. This can cause certain levels to become inaccessible. This utility will attempt to correct this."; // TODO-UPDATE: Localize
    public override bool IsAvailable => ViewModel.Editions.Any();
}