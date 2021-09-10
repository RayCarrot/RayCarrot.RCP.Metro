﻿using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Data model for a DosBox auto config file
    /// </summary>
    public class Emulator_DOSBox_AutoConfigData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Emulator_DOSBox_AutoConfigData()
        {
            // Create properties
            Configuration = new Dictionary<string, string>();
            SectionNames = new Dictionary<string, string[]>();
            CustomLines = new List<string>();
        }

        /// <summary>
        /// The configuration and their values
        /// </summary>
        public Dictionary<string, string> Configuration { get; }

        /// <summary>
        /// The section configuration keys for each section name
        /// </summary>
        public Dictionary<string, string[]> SectionNames { get; }

        /// <summary>
        /// The custom lines
        /// </summary>
        public List<string> CustomLines { get; }
    }
}