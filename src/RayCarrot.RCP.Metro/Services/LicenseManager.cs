using Microsoft.Win32;

namespace RayCarrot.RCP.Metro;

public class LicenseManager
{
    public bool HasAcceptedLicense()
    {
        // Get the license value, if one exists
        object? regValue = Registry.GetValue(AppFilePaths.RegistryBaseKey, AppFilePaths.RegistryLicenseValue, 0);

        if (regValue is not int intValue)
            return false;

        // Check if it has been accepted
        return intValue == 1;
    }

    public bool PrompLicense()
    {
        // Create license popup dialog
        LicenseDialog licenseDialog = new();

        // Show the dialog
        licenseDialog.ShowDialog();

        // Set Registry value if accepted
        if (licenseDialog.Accepted)
            Registry.SetValue(AppFilePaths.RegistryBaseKey, AppFilePaths.RegistryLicenseValue, 1);

        // Return if it was accepted
        return licenseDialog.Accepted;
    }
}