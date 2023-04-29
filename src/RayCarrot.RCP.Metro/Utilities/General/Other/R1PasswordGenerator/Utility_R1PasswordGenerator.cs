namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 password generator utility
/// </summary>
public class Utility_R1PasswordGenerator : Utility<Utility_R1PasswordGenerator_UI, Utility_R1PasswordGenerator_ViewModel>
{
    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.Utilities_R1Passwords_Header));
    public override GenericIconKind Icon => GenericIconKind.Utilities_R1PasswordGenerator;
}