namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Data for the <see cref="GamePatcher"/>
    /// </summary>
    public class GamePatcherData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="patchOffset">The offset to patch from</param>
        /// <param name="originalBytes">The original bytes to patch</param>
        /// <param name="patchedBytes">The bytes to replace the original ones with</param>
        /// <param name="fileSize">The expected file size of the file to patch</param>
        public GamePatcherData(uint patchOffset, byte[] originalBytes, byte[] patchedBytes, uint fileSize)
        {
            PatchOffset = patchOffset;
            OriginalBytes = originalBytes;
            PatchedBytes = patchedBytes;
            FileSize = fileSize;
        }

        /// <summary>
        /// The offset to patch from
        /// </summary>
        public uint PatchOffset { get; }

        /// <summary>
        /// The original bytes to patch
        /// </summary>
        public byte[] OriginalBytes { get; }

        /// <summary>
        /// The bytes to replace the original ones with
        /// </summary>
        public byte[] PatchedBytes { get; }

        /// <summary>
        /// The expected file size of the file to patch
        /// </summary>
        public uint FileSize { get; }
    }
}