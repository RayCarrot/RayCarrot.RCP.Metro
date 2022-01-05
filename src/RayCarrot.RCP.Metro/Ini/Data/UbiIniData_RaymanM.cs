#nullable disable
using System;

namespace RayCarrot.RCP.Metro.Ini
{
    /// <summary>
    /// Handles the Rayman M section of a ubi.ini file
    /// </summary>
    public class UbiIniData_RaymanM : UbiIniData
    {
        #region Constructor

        /// <summary>
        /// Constructor for a custom section name
        /// </summary>
        /// <param name="path">The path of the ubi.ini file</param>
        /// <param name="sectionKey">The name of the section to retrieve</param>
        public UbiIniData_RaymanM(string path, string sectionKey) : base(path, sectionKey)
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">The path of the ubi.ini file</param>
        public UbiIniData_RaymanM(string path) : base(path, SectionName)
        {
        }

        #endregion

        #region Constant Fields

        /// <summary>
        /// The section name
        /// </summary>
        public const string SectionName = "RAYMANM";

        #endregion

        // IDEA: Add properties for disc version installer

        #region Properties

        /// <summary>
        /// The Language key
        /// </summary>
        public string Language
        {
            get => Section?["Language"];
            set => Section["Language"] = value;
        }

        /// <summary>
        /// The Gli_Mode key
        /// </summary>
        public string GLI_Mode
        {
            get => Section?["Gli_Mode"];
            set => Section["Gli_Mode"] = value;
        }

        /// <summary>
        /// The Adapter key
        /// </summary>
        public string Adapter
        {
            get => Section?["Adapter"];
            set => Section["Adapter"] = value;
        }

        /// <summary>
        /// The TnL key
        /// </summary>
        public string TnL
        {
            get => Section?["TnL"];
            set => Section["TnL"] = value;
        }

        /// <summary>
        /// The TriLinear key
        /// </summary>
        public string TriLinear
        {
            get => Section?["TriLinear"];
            set => Section["TriLinear"] = value;
        }

        /// <summary>
        /// The Identifier key
        /// </summary>
        public string Identifier
        {
            get => Section?["Identifier"];
            set => Section["Identifier"] = value;
        }

        /// <summary>
        /// The Video_BPP key
        /// </summary>
        public string Video_BPP
        {
            get => Section?["Video_BPP"];
            set => Section["Video_BPP"] = value;
        }

        /// <summary>
        /// The Video_WantedQuality key
        /// </summary>
        public string Video_WantedQuality
        {
            get => Section?["Video_WantedQuality"];
            set => Section["Video_WantedQuality"] = value;
        }

        /// <summary>
        /// The Video_AutoAdjustQuality key
        /// </summary>
        public string Video_AutoAdjustQuality
        {
            get => Section?["Video_AutoAdjustQuality"];
            set => Section["Video_AutoAdjustQuality"] = value;
        }

        /// <summary>
        /// The VignettesFile key
        /// </summary>
        public string VignettesFile
        {
            get => Section?["VignettesFile"];
            set => Section["VignettesFile"] = value;
        }

        /// <summary>
        /// The TexturesFile key
        /// </summary>
        public string TexturesFile
        {
            get => Section?["TexturesFile"];
            set => Section["TexturesFile"] = value;
        }

        /// <summary>
        /// The TexturesCompressed key
        /// </summary>
        public string TexturesCompressed
        {
            get => Section?["TexturesCompressed"];
            set => Section["TexturesCompressed"] = value;
        }

        /// <summary>
        /// The Video_RealQuality key
        /// </summary>
        public string Video_RealQuality
        {
            get => Section?["Video_RealQuality"];
            set => Section["Video_RealQuality"] = value;
        }

        #endregion

        #region Formatted Properties

        /// <summary>
        /// The formatted Language key
        /// </summary>
        public RMLanguages? FormattedRMLanguage => Enum.TryParse(Language, out RMLanguages r2Languages) ? (RMLanguages?)r2Languages : null;

        /// <summary>
        /// The formatted GLI_Mode key
        /// </summary>
        public RayGLI_Mode FormattedGLI_Mode => RayGLI_Mode.Parse(GLI_Mode);

        /// <summary>
        /// The formatted TnL key
        /// </summary>
        public bool FormattedTnL
        {
            get => TnL != "0";
            set => TnL = value ? "1" : "0";
        }

        /// <summary>
        /// The formatted TriLinear key
        /// </summary>
        public bool FormattedTriLinear
        {
            get => TriLinear != "0";
            set => TriLinear = value ? "1" : "0";
        }

        /// <summary>
        /// The formatted Identifier key
        /// </summary>
        public Guid FormattedIdentifier => new Guid(Identifier);

        /// <summary>
        /// The formatted Video_BPP key
        /// </summary>
        public int? FormattedVideo_BPP => Int32.TryParse(Video_BPP, out int result) ? (int?)result : null;

        /// <summary>
        /// The formatted Video_WantedQuality key
        /// </summary>
        public int? FormattedVideo_WantedQuality => Int32.TryParse(Video_WantedQuality, out int result) ? (int?)result : null;

        /// <summary>
        /// The formatted Video_AutoAdjustQuality key
        /// </summary>
        public bool FormattedVideo_AutoAdjustQuality
        {
            get => Video_AutoAdjustQuality == "1";
            set => Video_AutoAdjustQuality = value ? "1" : "0";
        }

        /// <summary>
        /// The formatted TexturesCompressed key
        /// </summary>
        public bool FormattedTexturesCompressed
        {
            get => TexturesCompressed == "1";
            set => TexturesCompressed = value ? "1" : "0";
        }

        /// <summary>
        /// The formatted Video_RealQuality key
        /// </summary>
        public int? FormattedVideo_RealQuality => Int32.TryParse(Video_RealQuality, out int result) ? (int?)result : null;

        #endregion
    }
}