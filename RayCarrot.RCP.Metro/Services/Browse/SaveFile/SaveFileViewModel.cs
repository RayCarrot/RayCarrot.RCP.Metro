namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The model to use when saving a file
    /// </summary>
    public class SaveFileViewModel : BaseBrowseViewModel
    {
        /// <summary>
        /// The available extensions to save the file to
        /// </summary>
        public string Extensions { get; set; }
    }
}