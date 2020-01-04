using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RayCarrot.CarrotFramework;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.RCP.Metro;
using RayCarrot.UserData;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseRCPApp<MainWindow, AppUserData, RCPModdingAPIControllerManager>
    {
        #region Constructor

        public App() : base(true, "Img/Splash Screen.png")
        { }

        #endregion

        #region Protected Override Methods

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
                // Add loggers
                AddLoggers(DefaultLoggers.Console | DefaultLoggers.Debug | DefaultLoggers.Session, logLevel, builder => builder.AddProvider(new BaseLogProvider<FileLogger>())).
                // Add user data manager
                AddUserDataManager(() => new JsonBaseSerializer()
                {
                    Formatting = Formatting.Indented
                }).
                // Add the app view model
                AddAppViewModel<AppViewModel>().
                // Add localization manager
                AddLocalizationManager<RCPModdingLocalizationManager>().
                // Add API controller manager
                AddAPIControllerManager<RCPModdingAPIControllerManager>().
                // Build the framework
                Build(config);
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The current application
        /// </summary>
        public new static App Current => Application.Current as App;

        #endregion
    }
}