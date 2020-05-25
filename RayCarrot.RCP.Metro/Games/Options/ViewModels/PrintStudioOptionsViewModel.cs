using System;
using System.IO;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RayCarrot.IO;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 3 Print Studio options
    /// </summary>
    public class PrintStudioOptionsViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public PrintStudioOptionsViewModel()
        {
            // Create properties
            AsyncLock = new AsyncLock();

            // Get the .mms file path
            MMSFilePath = Games.PrintStudio.GetInstallDir() + "Run.MMS";

            var currentVersion = GetCurrentVersion();

            CanSetVersion = currentVersion != null;
            _selectedVersion = currentVersion ?? PrintStudioVersion.Version_03;

            RL.Logger?.LogInformationSource($"The current Print Studio version has been detected as {SelectedVersion} with the option to set the version as {CanSetVersion}");
        }

        #endregion

        #region Private Fields

        private PrintStudioVersion _selectedVersion;

        #endregion

        #region Private Properties

        /// <summary>
        /// The .mms file path
        /// </summary>
        private FileSystemPath MMSFilePath { get; }

        /// <summary>
        /// The async lock for <see cref="UpdatePrintStudioVersionAsync"/>
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected Print Studio version
        /// </summary>
        public PrintStudioVersion SelectedVersion
        {
            get => _selectedVersion;
            set
            {
                // Update value
                _selectedVersion = value;

                // Update data
                Task.Run(UpdatePrintStudioVersionAsync);
            }
        }

        /// <summary>
        /// Indicates if the version can be set
        /// </summary>
        public bool CanSetVersion { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current Print Studio version
        /// </summary>
        /// <returns>The current version or null if none was found</returns>
        public PrintStudioVersion? GetCurrentVersion()
        {
            try
            {
                // Read the file lines
                var lines = File.ReadAllLines(MMSFilePath);

                // Get indexes
                var firstIndex = lines[1].IndexOf("\"", StringComparison.Ordinal);
                var lastIndex = lines[1].LastIndexOf("\"", StringComparison.Ordinal);

                // Get the path substring
                var path = lines[1].Substring(firstIndex + 1, lastIndex - firstIndex - 1);

                // Set the current version
                if (path == "printstudio - 03.mrc")
                    return PrintStudioVersion.Version_03;
                else if (path == "printstudio - 05.mrc")
                    return PrintStudioVersion.Version_05;
                else
                    return null;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Print Studio version");

                return null;
            }
        }

        /// <summary>
        /// Updates the current Print Studio version based on the selected one
        /// </summary>
        /// <returns>The task</returns>
        public async Task UpdatePrintStudioVersionAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                try
                {
                    RL.Logger?.LogInformationSource("The Print Studio version is being updated...");

                    // Helper method for getting version tag
                    static string GetVersionTag(PrintStudioVersion version) =>
                        version switch
                        {
                            PrintStudioVersion.Version_03 => "03",
                            PrintStudioVersion.Version_05 => "05",
                            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
                        };

                    // Get the languages
                    var languages = new string[]
                    {
                        "DE",
                        "FR",
                        "IT",
                        "NL",
                        "SP",
                        "UK"
                    };

                    // Get the current version
                    var previousVersion = GetCurrentVersion() ?? throw new Exception("Previous Print Studio version can not be null");

                    RL.Logger?.LogInformationSource($"The current version is {previousVersion}");

                    // Make sure the version is different than the current one
                    if (previousVersion == SelectedVersion)
                    {
                        RL.Logger?.LogWarningSource("Print Studio version attempted to update when being the same");
                        return;
                    }

                    // Edit the .mms file
                    File.WriteAllLines(MMSFilePath, new string[]
                    {
                        "[STARTUP]",
                        "message= OPENBIGFILE 16376 N { \"printstudio - " + GetVersionTag(SelectedVersion) + ".mrc\" -1L }",
                        String.Empty, 
                        "[INCLUDE]",
                        "language.mms"
                    });

                    // Get the install directory
                    var installDir = Games.PrintStudio.GetInstallDir();

                    // Move existing files to the calender data
                    RCPServices.File.MoveFiles(new IOSearchPattern(installDir + @"Pictures\Common\calendars", SearchOption.TopDirectoryOnly, "Picture*"), installDir + "CalendarData" + GetVersionTag(previousVersion) + "Common", true);

                    foreach (var lang in languages)
                    {
                        RCPServices.File.MoveFiles(new IOSearchPattern(installDir + "Pictures" + lang + "calendars"), installDir + "CalendarData" + GetVersionTag(previousVersion) + lang, true);
                    }

                    // Move files from the calender data
                    RCPServices.File.MoveFiles(new IOSearchPattern(installDir + "CalendarData" + GetVersionTag(SelectedVersion) + "Common"), installDir + @"Pictures\Common\calendars", true);

                    foreach (var lang in languages)
                    {
                        RCPServices.File.MoveFiles(new IOSearchPattern(installDir + "CalendarData" + GetVersionTag(SelectedVersion) + lang), installDir + "Pictures" + lang + "calendars", true);
                    }

                    RL.Logger?.LogInformationSource($"The Print Studio version has been updated");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Updating Print Studio version");
                    await WPF.Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.PrintStudioOptions_VersionUpdateError);
                }
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The available Print Studio versions
        /// </summary>
        public enum PrintStudioVersion
        {
            /// <summary>
            /// The 2003 version
            /// </summary>
            Version_03,

            /// <summary>
            /// The 2005 version
            /// </summary>
            Version_05,
        }

        #endregion
    }
}