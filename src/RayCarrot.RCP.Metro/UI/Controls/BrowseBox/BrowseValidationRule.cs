namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The browse validation rules
    /// </summary>
    public enum BrowseValidationRule
    {
        /// <summary>
        /// No validation rule
        /// </summary>
        None,

        /// <summary>
        /// The path can not be empty
        /// </summary>
        NotEmpty,

        /// <summary>
        /// The path has to be an existing file or empty
        /// </summary>
        FileExists,

        /// <summary>
        /// The path has to be an existing file and not empty
        /// </summary>
        FileExistsAndNotEmpty,

        /// <summary>
        /// The path has to be an existing directory or empty
        /// </summary>
        DirectoryExists,

        /// <summary>
        /// The path has to be an existing directory and not empty
        /// </summary>
        DirectoryExistsAndNotEmpty
    }
}