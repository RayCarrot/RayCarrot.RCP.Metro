namespace RayCarrot.RCP.Metro;

/// <summary>
/// The archives utility
/// </summary>
public class Utility_Archives : Utility<Utility_Archives_Control, Utility_Archives_ViewModel>
{
    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.Utilities_ArchiveExplorer_Header));
    public override GenericIconKind Icon => GenericIconKind.Utilities_ArchiveExplorer;
}