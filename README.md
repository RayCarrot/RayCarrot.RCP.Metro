## Rayman Control Panel (Metro)
Rayman Control Panel (Metro) is the WPF redesign of the Rayman Control Panel. For the WinForms version (version 1.0.0 - 3.2.2), check here: [Rayman-Control-Panel](https://github.com/RayCarrot/Rayman-Control-Panel)

## Features
- Manage all PC Rayman games with a unified game launcher and configuration shortcuts.
- Configure the games with added options not normally available in the official configuration tools.
- Option to use fan-made utilities to improve the games, such as widescreen support, button remapping, higher quality cutscenes and more.
- Manage your Rayman save files by creating backups and converting to/from JSON.
- Customize how the program runs and looks, from the light/dark mode to more technical options.

### Configuration Tools
Configuration tools are available for the majority of games.

#### DosBox
For the DosBox games configuration a .ini file is created in the application's AppData directory where the chosen configuration values are saved. This file is then loaded into DosBox upon launching the game, along with the optional selected configuration file from the application settings and the mount path. The reason why all configuration commands are not passed in as parameters is due to DosBox having a parameter limit.

#### Rayman 2
The Rayman 2 configuration edits the ubi.ini file in the install directory if the GOG version is used, or else the ubi.ini file in the Windows directory. This configuration tool also has several utilities integrated into it.
The widescreen support option edits the main executable to have the game run in the same aspect ratio as the selected resolution. If widescreen support is disabled, the executable will be edited to restore the original value.
The custom button mapping edits the dinput.dll file in the game's install directory to allow for custom button mapping based on the user's selection.

#### Rayman M/Arena/3
Rayman M, Rayman Arena and Rayman 3 have their configuration saved in two ubi.ini files, one in the Windows directory and the other in the user's AppData. The configuration file the game reads usually depends on if the game is run as administrator or not. Due to this, the configuration tool for these games reads the configuration data from the primary file (in the Windows directory) and saves it to both files.

#### Rayman Raving Rabbids
Rayman Raving Rabbids stores its data in the Registry. Most of the values can be changed from within the game, thus they are not available in the configuration tool.

#### Rabbids Go Home
Rabbids Go Home sends the config as launch arguments when launching the game. As an option in the configuration this can be done in the Rayman Control Panel, allowing for a custom configuration.

#### Rayman Origins/Legends
Rayman Origins and Rayman Legends both store their data in the Registry.

#### Rayman Jungle Run
Rayman Jungle Run stores its configuration in single-byte files in the app data.

### Utilities
Utilities are available to help the games run better or add new functionality to them. Some files needed for utilities are not stored within the application due to them taking to much space. These will need to be downloaded.

#### Rayman 1 - PlayStation Soundtrack
This utility allows the game to run with the PlayStation soundtrack. For this to work, the Rayman Control Panel has to be running in the background to be able to turn off the in-game music and play the PlayStation one instead. This is done from a loop which keeps track of the game's values while running. The music itself is stores in the application's AppData and can be uninstalled from the utility dialog.

#### Rayman 1 - Complete Soundtrack
This utility replaces the Rayman Forever soundtrack with the complete one used in other editions. The utility also allows the soundtrack to be reverted back to the incomplete one.

#### Rayman Designer - Replace Infected Files
This utility will search the Rayman Designer install directory and replace any of the known infected files from the Rayman Forever release.

#### Rayman Designer - Create Missing Configuration File
This utility will recreate the Rayman Designer configuration file if it is missing, which in turn would cause the Mapper program not to launch.

#### Rayman 2 - Unofficial Translations
This utility will replace the game's textures.cnt and fix.sna files with custom ones to allow custom languages to be used in the game.

#### Rayman 2 - Patch Disc Version
This utility replaced the disc version's files with those from the GOG version to avoid the disc error.

#### Rayman 3 - DirectPlay
This utility will allow the option to enable or disable the Windows legacy feature DirectPlay which is needed to run the game.

#### Rayman Origins - Higher Quality Videos
This utility will allow the PC video files to be replaced with the ones from the PlayStation 3 version which are in higher quality.

#### Rayman Origins - Localization Converter
This utility allows the localization file to be converted to/from JSON.

#### Rayman Origins - Debug Commands
This utility will create a file called "cmdline.txt" in the game's install directory with the selected debug commands.

#### Rayman Origins - Update to 1.02
This utility will download the official Rayman Origins disc updater which allows the disc version to be updated to version 1.02.

#### Rayman Legends - UbiRay Character
This utility will edit the selected save file to have the currently selected character be UbiRay.

#### Rayman Legends - Localization Converter
This utility allows the localization file for Rayman Legends, Rayman Adventures and Rayman Mini to be converted to/from JSON.

#### Rayman Legends - Debug Commands
This utility will allow the game to be launched with the selected debug commands. This is only available for the Uplay version.

#### Rayman Fiesta Run - Localization Converter
This utility allows the localization file to be converted to/from JSON.

## Dependencies
The Rayman Control Panel uses these main dependencies:

- [RayCarrot.CarrotFramework](https://github.com/RayCarrot/Carrot-Framework)
- [RayCarrot.Rayman](https://github.com/RayCarrot/RayCarrot.Rayman)
- [RayCarrot.WPF](https://github.com/RayCarrot/Carrot-WPF)
- [RayCarrot.UserData](https://github.com/RayCarrot/Carrot-Framework-Extensions/tree/master/RayCarrot.UserData)
- [RayCarrot.Windows.Registry](https://github.com/RayCarrot/Carrot-Framework-Extensions/tree/master/RayCarrot.Windows.Registry)
- [RayCarrot.Windows.Shell](https://github.com/RayCarrot/Carrot-Framework-Extensions/tree/master/RayCarrot.Windows.Shell)
- [ByteSize](https://github.com/omar/ByteSize)
- [Costura.Fody](https://github.com/Fody/Costura)
- [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged)
- [ini-parser](https://github.com/rickyah/ini-parser)
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
- [NAudio.Vorbis](https://github.com/NAudio/Vorbis/)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Nito.AsyncEx](https://github.com/StephenCleary/AsyncEx)
- [XamlAnimatedGif](https://github.com/XamlAnimatedGif/XamlAnimatedGif)
- [Infralution.Localization.Wpf](https://www.codeproject.com/Articles/35159/WPF-Localization-Using-RESX-Files)
- [Resource.Embedder](https://github.com/MarcStan/Resource.Embedder)
- [Hardcodet.NotifyIcon.Wpf](http://www.hardcodet.net/wpf-notifyicon)
- [gong-wpf-dragdrop](https://github.com/punker76/gong-wpf-dragdrop)

## Localization
Starting with version 4.1.0 the Rayman Control Panel will support localization.

As of now the following strings are not localized:
- Version history
- App news (from server manifest)
- Exception messages (from Carrot Framework)
- Certain dialogs (from Carrot Framework)
- Game names
- Debug page
- License dialog

For more information and information on how to contribute, check out the Steam discussion page here:
[Rayman Control Panel - Localization](https://steamcommunity.com/groups/RaymanControlPanel/discussions/0/1812044473314212117/)

## Contact
You can contact me on the following places:

- [Twitter](https://twitter.com/RayCarrot)
- [Email](mailto:RayCarrotMaster@gmail.com)

## Screenshots

<div align="center">

<img alt="Screenshot1" src="https://raw.githubusercontent.com/RayCarrot/Rayman-Control-Panel-Metro/master/Screenshots/Screenshot1.png">

<img alt="Screenshot2" src="https://raw.githubusercontent.com/RayCarrot/Rayman-Control-Panel-Metro/master/Screenshots/Screenshot2.png">

<img alt="Screenshot3" src="https://raw.githubusercontent.com/RayCarrot/Rayman-Control-Panel-Metro/master/Screenshots/Screenshot3.png">

<img alt="Screenshot4" src="https://raw.githubusercontent.com/RayCarrot/Rayman-Control-Panel-Metro/master/Screenshots/Screenshot4.png">

<img alt="Screenshot5" src="https://raw.githubusercontent.com/RayCarrot/Rayman-Control-Panel-Metro/master/Screenshots/Screenshot5.png">

</div>

## Licence

[MIT License (MIT)](./LICENSE)