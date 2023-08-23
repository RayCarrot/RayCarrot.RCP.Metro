namespace RayCarrot.RCP.Metro;
// TODO-UPDATE: Remove support for multiple extensions. This can cause issues when a file name has multiple periods
//              in it. The reason for this class being made in the first place was for UbiArt IPK files containing files with
//              extensions such as .tga.ckd, where the .ckd (cooked) extension was appended to the normal one, but not the
//              extension we want to use (since it's not unique to that file type). A better fix for that might be to have the
//              IPK archive manager return the non-ckd file extension when using it to identify the file type, while still
//              keeping it in the name.

/// <summary>
/// A file extension
/// </summary>
public class FileExtension
{
    #region Constructors

    /// <summary>
    /// Constructor for a complete file extension or file name
    /// </summary>
    /// <param name="fileExtensions">The complete file extension or file name</param>
    public FileExtension(string fileExtensions) : this(fileExtensions, false) { }

    /// <summary>
    /// Constructor for a complete file extension or file name
    /// </summary>
    /// <param name="fileExtensions">The complete file extension or file name</param>
    /// <param name="multiple">Indicates if multiple file extensions are supported</param>
    [Obsolete("Use FileExtension(string fileExtensions) instead as support for multiple file extensions will be removed")]
    public FileExtension(string fileExtensions, bool multiple)
    {
        if (fileExtensions.IsNullOrWhiteSpace())
        {
            AllFileExtensions = Array.Empty<string>();
        }
        else if (multiple)
        {
            AllFileExtensions = fileExtensions.ToLowerInvariant().Split('.').Skip(1).Select(x => $".{x}").ToArray();
        }
        else
        {
            string? ext = System.IO.Path.GetExtension(fileExtensions.ToLowerInvariant());
            AllFileExtensions = ext.IsNullOrEmpty() ? Array.Empty<string>() : ext.YieldToArray();
        }
    }

    /// <summary>
    /// Constructor for a collection of file extensions
    /// </summary>
    /// <param name="fileExtensions">The file extensions</param>
    public FileExtension(IEnumerable<string> fileExtensions)
    {
        if (fileExtensions == null)
            throw new ArgumentNullException(nameof(fileExtensions));

        AllFileExtensions = fileExtensions.Select(x => x.ToLowerInvariant()).ToArray();
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The available file extensions, in order
    /// </summary>
    public string[] AllFileExtensions { get; }

    /// <summary>
    /// All file extensions, combined into one
    /// </summary>
    public string FileExtensions => String.Join(String.Empty, AllFileExtensions);

    /// <summary>
    /// The primary file extension
    /// </summary>
    public string PrimaryFileExtension => AllFileExtensions.LastOrDefault() ?? String.Empty;

    /// <summary>
    /// The display name for the extension
    /// </summary>
    public string DisplayName => FileExtensions.ToUpperInvariant();

    public string FileFilter => PrimaryFileExtension.IsNullOrWhiteSpace() ? "*" : $"*{PrimaryFileExtension}";

    /// <summary>
    /// Gets a file filter item for the file extension. This only includes the primary one.
    /// </summary>
    public FileFilterItem GetFileFilterItem => PrimaryFileExtension.IsNullOrWhiteSpace() ? new FileFilterItem("*", 
        // TODO: Localize
        "Files") : new FileFilterItem($"*{PrimaryFileExtension}", PrimaryFileExtension.Substring(1).ToUpperInvariant());

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets a new <see cref="FileExtensions"/> only containing the primary extension
    /// </summary>
    /// <returns>The primary file extension</returns>
    public FileExtension GetPrimaryFileExtension() => new FileExtension(PrimaryFileExtension);

    /// <summary>
    /// Checks if the other instance is equals to the current one
    /// </summary>
    /// <param name="other">The other instance to compare to the current one</param>
    /// <returns>True if the other instance is equals to the current one, false if not</returns>
    public bool Equals(FileExtension? other)
    {
        return FileExtensions == other?.FileExtensions;
    }

    /// <summary>
    /// True if the specified object equals the current instance
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        return obj is FileExtension path && Equals(path);
    }

    /// <summary>
    /// Returns the hash code for this instance
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance</returns>
    public override int GetHashCode()
    {
        return FileExtensions.GetHashCode();
    }

    /// <summary>
    /// Gets the <see cref="DisplayName"/> for the extension
    /// </summary>
    /// <returns>The extension display name</returns>
    public override string ToString() => DisplayName;

    #endregion

    #region Static Operators

    /// <summary>
    /// Checks if the two paths are the same
    /// </summary>
    /// <param name="a">The first path</param>
    /// <param name="b">The second path</param>
    /// <returns>True if they are the same, false if not</returns>
    public static bool operator ==(FileExtension? a, FileExtension? b)
    {
        if (a is null)
            return b is null;

        return a.Equals(b);
    }

    /// <summary>
    /// Checks if the two paths are not the same
    /// </summary>
    /// <param name="a">The first path</param>
    /// <param name="b">The second path</param>
    /// <returns>True if they are not the same, false if they are</returns>
    public static bool operator !=(FileExtension? a, FileExtension? b)
    {
        return !(a == b);
    }

    #endregion
}