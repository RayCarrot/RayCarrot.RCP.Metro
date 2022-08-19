namespace RayCarrot.RCP.Metro;

/// <summary>
/// The patch creator utility
/// </summary>
public class Utility_PatchCreator : Utility<Utility_PatchCreator_Control, Utility_PatchCreator_ViewModel>
{
    public override string DisplayHeader => Resources.PatchCreator_Title;
    public override GenericIconKind Icon => GenericIconKind.Utilities_PatchCreator;
}