using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the about page
    /// </summary>
    public class AboutPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AboutPageViewModel()
        {
            MainAboutText = "Rayman Control Panel is an open source community project created by RayCarrot. This software is a single free to use executable file with the intent to easily access settings and fixes for the various Rayman titles released for Windows." +
                            Environment.NewLine +
                            "This is a growing project with more features planned on being added with future updates. The utilities and various configuration tools have been created with help from various members of the Rayman community, all credited below.";

            Credits = new ObservableCollection<DuoGridItemViewModel>()
            {
                new DuoGridItemViewModel()
                {
                    Header = "RayCarrot  ",
                    Text = "User interface, Carrot Framework, game finder, game installer, utilities, Rayman 2 translations"
                },
                new DuoGridItemViewModel()
                {
                    Header = "Noserdog  ",
                    Text = "Controller patches"
                },
                new DuoGridItemViewModel()
                {
                    Header = "Juanmv94  ",
                    Text = "Rayman 2 button remapping"
                },
                new DuoGridItemViewModel()
                {
                    Header = "RibShark  ",
                    Text = "Cheat code list, general help with the utilities"
                },
                new DuoGridItemViewModel()
                {
                    Header = "PluMGMK  ",
                    Text = "Rayman 2 translations"
                },
                new DuoGridItemViewModel()
                {
                    Header = "Haruka Tavares  ",
                    Text = "Rayman 2 translations"
                },
                new DuoGridItemViewModel()
                {
                    Header = "Evelyn Chickentalk  ",
                    Text = "File structure for the education Rayman games"
                },
            };

            SpecialThanks = "Special thanks to AuToMaNiAk005, MixerX, ICUP321, PokGOT4N and all other Rayman fans!";

            // Create commands
            OpenUrlCommand = new RelayCommand(x => OpenUrl(x?.ToString()));
            ShowVersionHistoryCommand = new RelayCommand(ShowVersionHistory);
            CheckForUpdatesCommand = new AsyncRelayCommand(async () => await App.CheckForUpdatesAsync(true));
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The main about text
        /// </summary>
        public string MainAboutText { get; }

        /// <summary>
        /// The credits info items
        /// </summary>
        public ObservableCollection<DuoGridItemViewModel> Credits { get; }

        /// <summary>
        /// The special thanks text
        /// </summary>
        public string SpecialThanks { get; }

        #endregion

        #region Commands

        public ICommand OpenUrlCommand { get; }

        public ICommand ShowVersionHistoryCommand { get; }

        public ICommand CheckForUpdatesCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the specified url
        /// </summary>
        /// <param name="url">The url to open</param>
        public void OpenUrl(string url)
        {
            try
            {
                Process.Start(url)?.Dispose();
            }
            catch (Exception ex)
            {
                ex.HandleError($"Opening url {url}");
            }
        }

        /// <summary>
        /// Shows the application version history
        /// </summary>
        public void ShowVersionHistory()
        {
            new AppNewsDialog().ShowDialog();
        }

        #endregion
    }
}