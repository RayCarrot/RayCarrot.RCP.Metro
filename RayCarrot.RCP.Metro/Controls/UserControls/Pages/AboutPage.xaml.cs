using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class AboutPage : BaseUserControl<AboutPageViewModel>
    {
        public AboutPage()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// View model for the about page
    /// </summary>
    public class AboutPageViewModel : BaseRCPViewModel
    {
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
        }

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
    }
}