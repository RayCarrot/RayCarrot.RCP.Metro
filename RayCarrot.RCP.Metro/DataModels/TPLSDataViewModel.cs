using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Model for TPLS data
    /// </summary>
    public class TPLSDataViewModel : BaseViewModel
    {
        /// <summary>
        /// The path of the music file
        /// </summary>
        public FileSystemPath MusicFile => RCFRCP.Data.TPLSData.InstallDir + "Music.dat";

        /// <summary>
        /// Indicates if TPLS is running
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// The last retrieved X Axis value
        /// </summary>
        public short XAxis { get; set; }

        /// <summary>
        /// The last retrieved Y Axis value
        /// </summary>
        public short YAxis { get; set; }

        /// <summary>
        /// The last retrieved world value
        /// </summary>
        public string World { get; set; }

        /// <summary>
        /// The last retrieved level value
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// The last retrieved boss event value
        /// </summary>
        public bool BossEvent { get; set; }

        /// <summary>
        /// The last retrieved music value
        /// </summary>
        public bool Music { get; set; }

        /// <summary>
        /// The last retrieved value indicating if options is on
        /// </summary>
        public bool OptionsOn { get; set; }

        /// <summary>
        /// The last retrieved value indicating if options is off
        /// </summary>
        public bool OptionsOff { get; set; }

        /// <summary>
        /// The last retrieved in level value
        /// </summary>
        public bool InLevel { get; set; }
    }
}