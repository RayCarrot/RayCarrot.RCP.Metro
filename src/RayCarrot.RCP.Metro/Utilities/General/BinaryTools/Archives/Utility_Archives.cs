namespace RayCarrot.RCP.Metro;

/// <summary>
/// The archives utility
/// </summary>
public class Utility_Archives : Utility<Utility_Archives_UI, Utility_Archives_ViewModel>
{
    public override string DisplayHeader => Resources.Utilities_ArchiveExplorer_Header;
    public override GenericIconKind Icon => GenericIconKind.Utilities_ArchiveExplorer;
}