using IniParser.Model.Configuration;
using IniParser.Parser;

namespace RayCarrot.RCP.Metro.Ini
{
    /// <summary>
    /// The <see cref="IniDataParser"/> for a ubi.ini file
    /// </summary>
    public class UbiIniDataParser : IniDataParser
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UbiIniDataParser() : base(new IniParserConfiguration()
        {
            SkipInvalidLines = true,
            AllowDuplicateKeys = true,
            OverrideDuplicateKeys = true,
            CaseInsensitive = true
        })
        { }

        #endregion
    }
}