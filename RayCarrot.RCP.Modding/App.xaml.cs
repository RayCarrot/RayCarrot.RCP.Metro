using Microsoft.Extensions.Logging;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.RCP.Core.UI;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseRCPApp
    {
        public App() : base(false)
        {

        }

        /// <summary>
        /// Gets the main <see cref="Window"/> to show
        /// </summary>
        /// <returns>The Window instance</returns>
        protected override Window GetMainWindow()
        {
            return new MainWindow();
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

            new FrameworkConstruction().
                // TODO: Add file logger
                // Add loggers
                AddLoggers(DefaultLoggers.Console | DefaultLoggers.Debug | DefaultLoggers.Session, logLevel).
                // TODO: Replace with RCP exception handler
                // Add exception handler
                AddExceptionHandler<DefaultExceptionHandler>().
                // TODO: Replace with RCP message UI manager
                // Add message UI manager
                AddMessageUIManager<DefaultWPFMessageUIManager>().
                // Add browse UI manager
                AddBrowseUIManager<DefaultWPFBrowseUIManager>().
                // Add registry manager
                AddRegistryManager<DefaultRegistryManager>().
                // Add registry browse UI manager
                AddRegistryBrowseUIManager<DefaultWPFRegistryBrowseUIManager>().
                // Add the app view model
                AddSingleton(new AppViewModel()).
                // TODO: Add RCP file manager
                // Add a file manager
                //AddTransient<RCPFileManager>().
                // Add a dialog manager
                AddDialogBaseManager<RCPDialogBaseManager>().
                // Add RCP API
                AddRCPAPI<RCPModdingAPIControllerManager>(new APIControllerSettings().
                    // Add UI settings
                    SetUISettings(new RCPModdingAPIControllerUISettings())).
                // Build the framework
                Build(config, loadDefaultsFromDomain: false);
        }
    }
}