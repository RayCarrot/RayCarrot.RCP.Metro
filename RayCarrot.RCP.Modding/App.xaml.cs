using Microsoft.Extensions.Logging;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;
using System.Collections.Generic;
using Newtonsoft.Json;
using RayCarrot.RCP.Metro;
using RayCarrot.UserData;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseRCPApp<MainWindow>
    {
        public App() : base(true, "Img/Splash Screen.png")
        {

        }

        /// <summary>
        /// Sets up the framework with loggers and other services
        /// </summary>
        /// <param name="config">The configuration values to pass on to the framework, if any</param>
        /// <param name="logLevel">The level to log</param>
        /// <param name="args">The launch arguments</param>
        protected override void SetupFramework(IDictionary<string, object> config, LogLevel logLevel, string[] args)
        {
            // Add custom configuration
            config.Add(RCFIO.AutoCorrectPathCasingKey,
                // NOTE: Set to true?
                false);

            // TODO: Add user data manager
            new FrameworkConstruction().
                // TODO: Add file logger
                // Add loggers
                AddLoggers(DefaultLoggers.Console | DefaultLoggers.Debug | DefaultLoggers.Session, logLevel).
                // Add exception handler
                AddExceptionHandler<RCPExceptionHandler>().
                // Add user data manager
                AddUserDataManager(() => new JsonBaseSerializer()
                {
                    Formatting = Formatting.Indented
                }).
                // Add message UI manager
                AddMessageUIManager<RCPMessageUIManager>().
                // Add browse UI manager
                AddBrowseUIManager<DefaultWPFBrowseUIManager>().
                // Add registry manager
                AddRegistryManager<DefaultRegistryManager>().
                // Add registry browse UI manager
                AddRegistryBrowseUIManager<DefaultWPFRegistryBrowseUIManager>().
                // Add the app view model
                AddAppViewModel<AppViewModel>().
                // Add a file manager
                AddFileManager<RCPFileManager>().
                // Add a dialog manager
                AddDialogBaseManager<RCPDialogBaseManager>().
                // TODO: Add update manager
                // Add updater manager
                //AddUpdateManager<>().
                // TODO: Add app paths
                // Add application paths
                //AddApplicationPaths<RCPMetroApplicationPaths>().
                // Add localization manager
                AddLocalizationManager<RCPModdingLocalizationManager>().
                // Add API controller manager
                AddAPIControllerManager<RCPModdingAPIControllerManager>().
                // Build the framework
                Build(config, loadDefaultsFromDomain: false);
        }
    }
}