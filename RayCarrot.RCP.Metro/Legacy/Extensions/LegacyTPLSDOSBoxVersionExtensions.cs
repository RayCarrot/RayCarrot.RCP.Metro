namespace RayCarrot.RCP.Metro.Legacy
{
    /// <summary>
    /// Extension methods for <see cref="LegacyTPLSDOSBoxVersion"/>
    /// </summary>
    public static class LegacyTPLSDOSBoxVersionExtensions
    {
        /// <summary>
        /// Get the current TPLS DosBox version from the specified <see cref="LegacyTPLSDOSBoxVersion"/>
        /// </summary>
        /// <param name="version">The legacy TPLS DosBox version to get the current version for</param>
        /// <returns>The current TPLS DosBox version or null if not found</returns>
        public static TPLSDOSBoxVersion? GetCurrent(this LegacyTPLSDOSBoxVersion version)
        {
            switch (version)
            {
                case LegacyTPLSDOSBoxVersion.DOSBox_0_74:
                    return TPLSDOSBoxVersion.DOSBox_0_74;

                case LegacyTPLSDOSBoxVersion.DOSBox_SVN_Daum:
                    return TPLSDOSBoxVersion.DOSBox_SVN_Daum;

                default:
                    return null;
            }
        }
    }
}