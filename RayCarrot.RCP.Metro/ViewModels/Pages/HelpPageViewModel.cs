using System;
using System.Collections.ObjectModel;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;
// ReSharper disable StringLiteralTypo

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the help page
    /// </summary>
    public class HelpPageViewModel : BaseRCPViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public HelpPageViewModel()
        {
            HelpItems = new ObservableCollection<HelpItemViewModel>()
            {
                // Rayman Control Panel
                new HelpItemViewModel()
                {
                    DisplayHeader = "Rayman Control Panel",
                    SubItems = new ObservableCollection<HelpItemViewModel>()
                    {
                        // Updates
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Updates",
                            HelpText = "The program will by default check for updates on launch, unless the option has been disabled. This is done in the background and might take a few seconds. " +
                                       "Updates can manually be checked for in the settings page." +
                                       Environment.NewLine +
                                       Environment.NewLine +
                                       "If an error occurs with the update service it can manually be downloaded from:" +
                                       Environment.NewLine +
                                       "http://raycarrot.ylemnova.com/"
                        },

                        // Compatibility
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Compatibility",
                            HelpText = "The minimum required version to run the program is Windows Vista, with Windows 7 or above being recommended."
                        },

                        // Game Installer
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Game Installer",
                            HelpText = "For games which support to be installed from a disc you can do so from the drop down menu of the game. Currently " +
                                       "Rayman 2, Rayman M and Rayman Arena are the only supported games." +
                                       Environment.NewLine +
                                       Environment.NewLine +
                                       "During the installation you will have to specify a directory to install to. The game will install in a sub-directory in " +
                                       "the specified directory. For example, you choose to install Rayman 2 under C:\\Ubisoft it will get installed under " +
                                       "C:\\Ubisoft\\Rayman 2" +
                                       Environment.NewLine +
                                       Environment.NewLine +
                                       "For Rayman 2 the installer will replace the executable file with the one from the GOG version to allow the game to run without " +
                                       "inserting the disc. This is done to avoid a common disc error which usually occurs during the later half of the game." +
                                       Environment.NewLine +
                                       Environment.NewLine +
                                       "To uninstall one of the games installed using the game installer you simply have to delete the directory and its files. The game " +
                                       "will not show up under installed programs due to not having an uninstaller."
                        },

                        // Backup Games
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Backup/Restore",
                            HelpText = "The program has a backup and restore feature for all supported games. The backups are by default stored in the " +
                                       "documents folder, but the location can be changed in the settings." +
                                       Environment.NewLine +
                                       Environment.NewLine +
                                       "The backups themselves are always stored in the " +
                                       $"'{AppViewModel.BackupFamily}' sub-directory. It is not recommended to manually modify these files."
                        },

                        // App Data Location
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "App Data Location",
                            HelpText = "The program saves its app data mainly in the current user's local app data, with some settings stored in the Registry. Below is a complete list" +
                                       "of the locations where data is stored." +
                                       Environment.NewLine +
                                       Environment.NewLine +
                                       $"• Main application data - {CommonPaths.UserDataBaseDir}" +
                                       Environment.NewLine +
                                       $"• Registry Settings - {CommonPaths.RegistryBaseKey}" +
                                       Environment.NewLine +
                                       $"• Framework Registry Settings - {RCFRegistryPaths.RCFBasePath}" +
                                       Environment.NewLine +
                                       $"• Temporary data - {CommonPaths.TempPath}",
                            RequiredUserLevel = UserLevel.Advanced
                        },

                        // Launch arguments
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Launch Arguments",
                            HelpText = "The Rayman Control Panel supports several launch arguments, mainly meant for debugging the application. Below is a complete list of the available" +
                                       "launch arguments. The '-' character should always be included. The '{}' characters show that a custom variable can be used, in which case the '{}'" +
                                       "should not be included." +
                                       Environment.NewLine +
                                       Environment.NewLine +
                                       "-reset (Resets all app data before launch)" +
                                       Environment.NewLine +
                                       "-install {filePath} (Removes the installer from the specified path)" +
                                       Environment.NewLine +
                                       "-ul {userLevel} (sets the user level once the framework is built)" +
                                       Environment.NewLine +
                                       "-loglevel {logLevel} (sets the log level to log, default is Information)",
                            RequiredUserLevel = UserLevel.Technical
                        },

                        // Launch arguments
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Debugging",
                            HelpText = "If a debugger is attached to the program the log viewer will automatically open. This can manually be opened from the debug " +
                                       "page even without a debugger being attached. If the debug user level is not enabled, the log can still be viewed from the file " +
                                       "it writes to under the following path:" +
                                       Environment.NewLine +
                                       CommonPaths.LogFile,
                            RequiredUserLevel = UserLevel.Debug
                        },
                    }
                },

                // Games
                new HelpItemViewModel()
                {
                    DisplayHeader = "Games",
                    SubItems = new ObservableCollection<HelpItemViewModel>()
                    {
                        // General
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "General",
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                // Game not Launching
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "Game not Launching",
                                    HelpText = "Here are the two most common things to try out if a game won't launch:" +
                                               Environment.NewLine +
                                               Environment.NewLine +
                                               "*Run the game as administrator:" +
                                               Environment.NewLine +
                                               "The option to run a program as administrator can be found in the context menu by right-clicking the program." +
                                               Environment.NewLine +
                                               Environment.NewLine +
                                               "*Run the game under compatibility mode:" +
                                               Environment.NewLine +
                                               "Running an application under compatibility mode is done by selecting the option under the compatibility options " +
                                               "in the file properties. For many older games it is recommended to select Windows XP Service Pack 2 or 3."
                                },
                            }
                        },

                        // Rayman 1
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman 1",
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                // Dos Emulator
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "MS-Dos Emulator",
                                    HelpText = "Rayman 1, and most of its PC spin-offs (including Rayman Designer, Rayman by his Fans and Rayman 60 Levels) " +
                                               "are MS-DOS programs and are not compatible with modern versions of Windows. Running them requires a DOS emulator, such as " +
                                               "DosBox. Currently DosBox is the only supported emulator by the Rayman Control Panel."
                                },

                                // Installation failed
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "Installation Failed",
                                    HelpText = "If one of the Rayman 1 based installers fail when it has finished installing it will delete the installed game if you " +
                                               "cancel in the installer. To prevent this, force close the installer using task manager. The game will most likely still be installed. " +
                                               "If any files are missing, copy them over from the disc manually."
                                },

                                // Rayman Designer Editor
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "Rayman Designer Editor",
                                    HelpText = "Rayman Designer, also known as Rayman Gold or Rayman's World, is the Rayman game which includes the Mapper program which is " +
                                               "used to create your own levels. The Mapper is a normal Windows executable file and will run without the need of an emulator. All of the static " +
                                               "parts of the level is created in the Mapper program, while all of the so-called 'events' are placed in the event editor (found within " +
                                               "the game itself)."
                                },

                                // Importing Maps
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "Importing Maps",
                                    HelpText = "Importing maps is done in the Mapper program. If an error occurs when doing so you can try moving the map file to the " +
                                               "RayKit directory and importing it from there."
                                },
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman 2",
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "Game Running too Fast",
                                    HelpText = "If Rayman 2 is running too fast while using nGlide, change the refresh rate to 120hz. This should be done even on monitors " +
                                               "which do not support 120hz."
                                },
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "60 vs 30 fps",
                                    HelpText = "Some parts of the game may not work if the game is running in 60fps. The most notable issue is during the bonus games."
                                },
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "No Disc Error",
                                    HelpText = "On disc versions of the game there is a known issue where the game will display a CD error during gameplay. " +
                                               "There is currently no solution to this issue other than using a digital version, such as the GOG version, instead."
                                },
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman M/Arena",
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "Missing Textures",
                                    HelpText = "If textures are missing in the game while using an Intel graphics card, try turning off Transform and Lightning."
                                },
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman 3",
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "Missing Textures",
                                    HelpText = "If textures are missing in the game while using an Intel graphics card, try turning off Transform and Lightning."
                                },
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman Raving Rabbids",
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "jade_enr.exe Error",
                                    HelpText = "There is currently no fix for this error. This appears in all versions, including the GOG version, on certain computers."
                                },
                            }
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman Legends",
                            SubItems = new ObservableCollection<HelpItemViewModel>()
                            {
                                new HelpItemViewModel()
                                {
                                    DisplayHeader = "Error during load",
                                    HelpText = "The error occurs mainly on Windows 10 devices due to the game not being able to read/write to the game save file and/or the game configuration settings. The most common fix involves adding Rayman Legends as an exception to the Controlled Folder Access section of the built-in Windows Security system."
                                },
                            }
                        },
                    }
                },

                // Cheat Codes
                new HelpItemViewModel()
                {
                    DisplayHeader = "Cheat Codes",
                    SubItems = new ObservableCollection<HelpItemViewModel>()
                    {
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman 1",
                            HelpText = "• 99 lives  -  type trj8p or raylives or [TAB]bertrand during gameplay" +
                                       Environment.NewLine +
                                       "• All fist power-ups  -  type 2x2rmfmf or goldfist or [TAB]antoine during gameplay" +
                                       Environment.NewLine +
                                       "• All normal powers  -  type en5gol2g or power or [TAB]benoit during gameplay" +
                                       Environment.NewLine +
                                       "• Full health  -  type kom0ogdk or raypoint or [TAB]christ during gameplay" +
                                       Environment.NewLine +
                                       "• 10 added Tings  -  type 86e40g91 or raywiz or [TAB]jojo during gameplay" +
                                       Environment.NewLine +
                                       "• Skip current level section  -  type o8feh or winmap or [TAB]cyril during gameplay" +
                                       Environment.NewLine +
                                       "• Display hidden message  -  type [TAB]program during gameplay" +
                                       Environment.NewLine +
                                       "• Free movement  -  type [TAB];overay[BACKSPACE] during gameplay" +
                                       Environment.NewLine +
                                       "• 100MHz Refresh rate  -  type freq10 on the map" +
                                       Environment.NewLine +
                                       "• 80MHz Refresh rate  -  type freq80 on the map" +
                                       Environment.NewLine +
                                       "• All normal powers  -  type power or [TAB]benoit on the map" +
                                       Environment.NewLine +
                                       "• Lens effect  -  type lens on the map" +
                                       Environment.NewLine +
                                       "• Unlock all levels  -  type 4ctrepfj or alworld or [TAB]francois on the map" +
                                       Environment.NewLine +
                                       "• Enter Breakout minigame  -  type b76b7081 or cbray or [TAB]olivier on the map once Mr Dark's Dare has been completed" +
                                       Environment.NewLine +
                                       "• Enter random stage from Breakout minigame  -  type [TAB]cbrayal[BACKSPACE] on the map" +
                                       Environment.NewLine +
                                       "• Stage selection  -  type [TAB]alevel[BACKSPACE] on the map"
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman Designer + Spin-Offs",
                            HelpText = "• 5 lives  -  type [TAB]lives05[BACKSPACE] during gameplay" +
                                       Environment.NewLine +
                                       "• 20 lives  -  type [TAB]lives20[BACKSPACE] during gameplay" +
                                       Environment.NewLine +
                                       "• 50 lives  -  type [TAB]lives50[BACKSPACE] during gameplay" +
                                       Environment.NewLine +
                                       "*All fist power-ups  -  type [TAB]goldens during gameplay" +
                                       Environment.NewLine +
                                       "• Finish level  -  type [TAB]finishing during gameplay" +
                                       Environment.NewLine +
                                       "• Full health  -  type [TAB]points during gameplay" +
                                       Environment.NewLine +
                                       "• Map number display  -  type [TAB]map[BACKSPACE] during gameplay" +
                                       Environment.NewLine +
                                       "• Free movement  -  type [TAB]moveray[BACKSPACE] during gameplay" +
                                       Environment.NewLine +
                                       "• Unlock all levels  -  type [TAB]openall[BACKSPACE] on the map"
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman 2",
                            HelpText = "• Upgrade magic fist  -  type glowfist on the pause screen" +
                                       Environment.NewLine +
                                       "• Restore health  -  type gimmelife on the pause screen" +
                                       Environment.NewLine +
                                       "• Gain 5 Yellow Lums  -  type gimmelumz on the pause screen" +
                                       Environment.NewLine +
                                       "• Go to any level  -  type gothere on the pause screen, select a level with the arrow keys and press enter" +
                                       Environment.NewLine +
                                       "• Unlock grappling power  -  type hangon on the pause screen" +
                                       Environment.NewLine +
                                       "• Gain maximum health  -  press and release the J key to the rhythm of the Rayman 2 theme" +
                                       Environment.NewLine +
                                       "• Enter bonus level without all Lums and Cages  -   press the A, Numpad 0, Q, W, Q, W in order followed by enter on the Access Denied screen" +
                                       Environment.NewLine +
                                       "*Skip cutscenes  -  type NOMOVIES on the pause screen" +
                                       Environment.NewLine +
                                       "• New loading screens  -  type ALLVIGN on the pause screen" +
                                       Environment.NewLine +
                                       "• Disable Murfy  -  type NOMOREMURFY on the pause screen" +
                                       Environment.NewLine +
                                       "• Access hidden area in Tomb of the Ancients  -  In the final part of the Tomb of the Ancients, " +
                                       "stand on the third panel away from the entrance to the Technical Check-up where Rayman fights Clark. " +
                                       "Type PLAYJEFF and jump on the crate to the hole in the wall." +
                                       Environment.NewLine +
                                       "• Access Menezis  -  type SHOOTEMUP during the credits" +
                                       Environment.NewLine +
                                       "• Unknown  -  type GETELIX on the pause screen"
                        },
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman M/Arena",
                            HelpText = "• Skip cutscenes  -  type esc during the cutscene" +
                                       Environment.NewLine +
                                       "• Unlock all battle levels  -  enter ALLFISH as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Unlock all race levels  -  enter ALLTRIBES as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Unlock all levels  -  enter ALLRAYMANM as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Unlock all characters  -  enter PUPPETS as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Unlock all skins  -  enter CARNIVAL as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Unlock all battle levels in mode 1  -  enter ARENAS as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Unlock all race levels in mode 1  -  enter TRACKS as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Unlock all levels in mode 1  -  enter FIELDS as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Ragtime music in races  -  enter OLDTV as a profile name and press Shift + Ctrl + Alt" +
                                       Environment.NewLine +
                                       "• Reverse map  -  type reverse on the keyboard during gameplay" +
                                       Environment.NewLine +
                                       Environment.NewLine +
                                       "• Music team:" +
                                       Environment.NewLine +
                                       "To activate it, enter race mode and pause the game. On the pause screen, " +
                                       "hold L1 + R1 (or equivalent on the currently used controller), then press the optimize button. " +
                                       "Five blue balls will appear at the bottom of the screen. Release everything and press Jump. " +
                                       "The player should hear a high pitched \"Ding\" sound, and after 0.5 seconds, the first ball " +
                                       "should turn yellow. As soon as the ball turns yellow, hold Jump. The second ball should then turn yellow. " +
                                       "As soon as the second ball turns yellow, release Jump. The third ball should then turn yellow. " +
                                       "As soon as the third ball turns yellow, hold Jump again. The fourth ball should then turn yellow. " +
                                       "As soon as the fourth ball turns yellow, release Jump. The player should be taken to the \"Music Mode\" " +
                                       "screen where the player will be able to choose \"Music Team\"."
                        },             
                        new HelpItemViewModel()
                        {
                            DisplayHeader = "Rayman 3",
                            HelpText = "• Skip cutscenes  -  type esc during the cutscene" +
                                       Environment.NewLine +
                                       "• Reverse map  -  type reverse on the keyboard during gameplay"
                        },
                    }
                }
            };
        }

        /// <summary>
        /// The help items
        /// </summary>
        public ObservableCollection<HelpItemViewModel> HelpItems { get; }

        /// <summary>
        /// The current help item
        /// </summary>
        public HelpItemViewModel CurrentHelpItem { get; set; }
    }
}