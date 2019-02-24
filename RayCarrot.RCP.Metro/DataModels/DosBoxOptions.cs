using System.Collections.Generic;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Options for a DosBox game
    /// </summary>
    public class DosBoxOptions
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DosBoxOptions()
        {
            MountPath = FileSystemPath.EmptyPath;
            ConfigCommands = new Dictionary<string, string>()
            {
                {
                    DosBoxConfigViewModel.MemorySizeKey, "30"
                },
                {
                    DosBoxConfigViewModel.FrameskipKey, "0"
                },
                {
                    DosBoxConfigViewModel.CyclesKey, "20000"
                },
            };
            Commands = new string[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The file or directory to mount
        /// </summary>
        public FileSystemPath MountPath { get; set; }

        /// <summary>
        /// The DosBox config commands to set before launching the game
        /// </summary>
        public Dictionary<string, string> ConfigCommands { get; set; }

        /// <summary>
        /// The DosBox commands to run before launching the game
        /// </summary>
        public string[] Commands { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all commands
        /// </summary>
        /// <returns>The commands</returns>
        public IEnumerable<string> GetCommands()
        {
            foreach (var command in ConfigCommands)
                yield return $"CONFIG -set '{command.Key}={command.Value}'";

            foreach (var command in Commands)
                yield return command;
        }

        #endregion
    }
}