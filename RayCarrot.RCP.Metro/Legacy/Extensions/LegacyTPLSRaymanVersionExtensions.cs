namespace RayCarrot.RCP.Metro.Legacy
{
    /// <summary>
    /// Extension methods for <see cref="LegacyTPLSRaymanVersion"/>
    /// </summary>
    public static class LegacyTPLSRaymanVersionExtensions
    {
        /// <summary>
        /// Get the current TPLS Rayman version from the specified <see cref="LegacyTPLSRaymanVersion"/>
        /// </summary>
        /// <param name="version">The legacy TPLS Rayman version to get the current version for</param>
        /// <returns>The current TPLS Rayman version or null if not found</returns>
        public static TPLSRaymanVersion? GetCurrent(this LegacyTPLSRaymanVersion version)
        {
            switch (version)
            {
                case LegacyTPLSRaymanVersion.Auto:
                    return TPLSRaymanVersion.Auto;

                case LegacyTPLSRaymanVersion.Ray_1_12:
                    return TPLSRaymanVersion.Ray_1_12_0;

                case LegacyTPLSRaymanVersion.Ray_1_20:
                    return TPLSRaymanVersion.Ray_1_20;

                case LegacyTPLSRaymanVersion.Ray_1_21:
                    return TPLSRaymanVersion.Ray_1_21;

                default:
                    return null;
            }
        }
    }
}