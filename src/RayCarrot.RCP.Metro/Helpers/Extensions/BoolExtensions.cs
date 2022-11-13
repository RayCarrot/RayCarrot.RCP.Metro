namespace RayCarrot.RCP.Metro;

public static class BoolExtensions
{
    public static LocalizedString ToLocalizedString(this bool value)
    {
        return new ResourceLocString(value ? nameof(Resources.Bool_True) : nameof(Resources.Bool_False));
    }
}