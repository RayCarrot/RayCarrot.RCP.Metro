using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Designer create config utility
    /// </summary>
    public class Utility_RaymanDesigner_CreateConfig_ViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Utility_RaymanDesigner_CreateConfig_ViewModel()
        {
            // Create commands
            CreateConfigCommand = new AsyncRelayCommand(CreateConfigAsync);
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Commands

        public ICommand CreateConfigCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path to the config file
        /// </summary>
        public FileSystemPath ConfigPath => Games.RaymanDesigner.GetInstallDir() + "Ubisoft" + "ubi.ini";

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the Rayman Designer configuration file
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateConfigAsync()
        {
            // Get the path
            var path = ConfigPath;

            // Check if the file exists
            if (path.FileExists)
            {
                if (!await Services.MessageUI.DisplayMessageAsync(Resources.RDU_CreateConfig_ReplaceQuestion, Resources.RDU_CreateConfig_ReplaceQuestionHeader, MessageType.Question, true))
                    return;
            }

            try
            {
                Directory.CreateDirectory(path.Parent);

                // Create the file
                File.WriteAllLines(path, new string[]
                {
                    "[OSD]",
                    "Directory =.\\ubisoft\\osd",
                    "Valid = TRUE",
                    String.Empty,
                    "[INSTALLED PRODUCTS]",
                    "RAYKIT",
                    String.Empty,
                    "[RAYKIT]",
                    "SrcDataPath =\\",
                    "Directory =.\\"
                });

                Logger.Info($"The Rayman Designer config file has been recreated");

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.RDU_CreateConfig_Success);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying RD config patch");
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.RDU_CreateConfig_Error);
            }
        }

        #endregion
    }
}