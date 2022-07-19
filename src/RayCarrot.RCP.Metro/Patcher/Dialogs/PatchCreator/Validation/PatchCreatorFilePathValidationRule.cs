using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchCreatorFilePathValidationRule : ValidationRule
{
    private static readonly char[] _invalidPathCharacters = Path.GetInvalidPathChars();

    public AvailableFileLocationsWrapper? AvailableFileLocations { get; set; }

    // TODO-UPDATE: Localize
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string str || str.IsNullOrWhiteSpace())
            return new ValidationResult(false, "The path can not be empty");

        if (str.Any(x => _invalidPathCharacters.Contains(x)))
            return new ValidationResult(false, "The path can not contain invalid characters");

        string normalizedStr = str.ToLowerInvariant().Replace('/', '\\');

        if (AvailableFileLocations?.Value?.Any(x => x.Location.Replace('/', '\\').ToLowerInvariant() == normalizedStr) == true)
            return new ValidationResult(false, "The path can not be the same as an archive file");

        return ValidationResult.ValidResult;
    }
}