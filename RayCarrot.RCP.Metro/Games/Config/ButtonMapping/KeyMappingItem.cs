namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A key mapping item
    /// </summary>
    public class KeyMappingItem
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="originalKey">The original key</param>
        /// <param name="newKey">The new key</param>
        public KeyMappingItem(int originalKey, int newKey)
        {
            OriginalKey = originalKey;
            NewKey = newKey;
        }

        /// <summary>
        /// The original key
        /// </summary>
        public int OriginalKey { get; }

        /// <summary>
        /// The new key
        /// </summary>
        public int NewKey { get; }
    }
}