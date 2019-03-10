using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Data model for a DosBox auto config file
    /// </summary>
    public class DosBoxAutoConfigData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DosBoxAutoConfigData()
        {
            // Create properties
            Commands = new Dictionary<string, string>();
            CustomLines = new List<string>();
        }

        /// <summary>
        /// The commands
        /// </summary>
        public Dictionary<string, string> Commands { get; }

        /// <summary>
        /// The custom lines
        /// </summary>
        public List<string> CustomLines { get; }
    }
}