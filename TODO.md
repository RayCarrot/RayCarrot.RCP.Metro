# TODO
This document contains a list of planned features to be added in the Rayman Control Panel, in no particular order. Due to the scope of this a lot of these might never have a chance to actually be implemented. I do however appreciate any help I can get, so if anyone wants to help out then feel free to contact me or leave a PR!

## 🖥️ Version 15.0 - Core Update
Version 15.0 will see the app being migrated to the latest .NET version (currently .NET 8). Due to .NET not being pre-installed with Windows it means we need to change how the app is installed by the user. The current plan is to make the app fully installable using an installer such as [Inno Setup](https://jrsoftware.org/isinfo.php) which will then install any required dependencies. The updater will then instead download the latest installer file from the GitHub releases.

### Changes
- Migrate to .NET 8
- Deploy app using an installer
- Remove custom updater and replace by downloading latest installer file
- Remove custom uninstaller and rely on the installer being able to uninstall the app
- The way Windows APIs are called will need to be updated ([docs](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-enhance))

### Portable version?
Can we also offer a portable version where the user must manually install dependencies such as .NET? How will the update process work then? If portable then store app data locally in the folder?

## 🔗 Website
Update the [Rayman Control Panel website](https://raym.app/rcp/) to look nicer. Maybe even allow some utilities to be used on there directly, such as the Rayman 1 password generator?

## 👁️‍🗨️ Avalonia UI
Long-term it would be beneficial to migrate the app to a cross-platform framework such as Avalonia UI. This would allow for a native Linux version, which has been highly requested, and to have the app take advantage of the performance improvements which come with a newer framework. This would however be a major task since essentially all of the UI would need to be updated, especially the styles which would all need to be rewritten.

## 🎮 Replace Archive Explorer with Game Explorer
The current version of the Archive Explorer allows viewing and modifying archive files in a game installation, but not any other files. This poses limitations for games where assets may be unpacked, but still stored in proprietary formats which we want to allow the user to be able to convert and edit.

The solution is the *Game Explorer* which will act as a file/folder view of the game installation, while also having support for accessing the contents of archives, as well as other files with packed assets. It should use a generic node system which allows different types of data be represented, making it also usable for non-file based games like Rayman 3 GBA. Each node can have one or more editors associated with it, allowing direct editing for game file formats.

The node types and editors should be defined in a modular way, perhaps using components. DataNode can have list of components. These can define editors, UI etc. Some common reusable UIs will exist like FolderView for listing files. Data nodes have options if they appear in the tree view or not.

### Changes
- Replace the Archive Explorer with the Game Explorer
- Show serialization log like in [Psychonauts Studio](https://github.com/RayCarrot/PsychonautsStudio)
- Use [ImGui](https://github.com/ImGuiNET/ImGui.NET/tree/master/src/ImGui.NET.SampleProgram) to allow displaying and editing binary data
- Integrate mod loader into it (instead of saving changes you can create a mod from changes)
- Allow opening files with in-app editors or external programs
```
Open with -> 
    Cooked Texture Editor
    Binary Editor
Open with (external) ->
```

## 🧑‍💻 Code Cleanup
- Remove remaining `#nullable disable` - these were temporarily added to every file when migrating the codebase to globally enabling nullable reference types
- Be more consistent with usage of namespaces
- Replace `Fody.PropertyChanged` with `MVVMToolkit` using partial properties - this requires every ViewModel class to be updated
- Split localization sheet into multiple sheets for things like game titles, level names, Mod Loader, Archive Explorer etc.
- Find a solution to dealing with singleton instances and services - currently there's a half-implemented Dependency Injection system, but it's very inconsistently used
- Clean up custom styles
- Remove LocalizedString and force app restart upon language change - this should improve performance since the way LocalizedString works puts a lot of pressure on the Garbage Collector.
- Rename the repo to something more familiar, such as `rayman-control-panel`
- Rename the main namespace from `RayCarrot.RCP.Metro` to something like `RCP` or `RaymanControlPanel`
- Cache an int in the GameInstallation data for sorting the order. This updates in GamesManager when loaded and then each time the collection is updated. This allows us to sort without using the actual list from appdata.

## 🎨 User Interface
- Potentially move to some other UI framework since a lot of the MahApps.Metro styles have been completely redone. Alternatives are [WPF UI](https://github.com/lepoco/wpfui) and [Material Design](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit). Both however have issues.
- Use virtualizing more in ListBox and ItemsControl, such as the Games page and Archive Explorer. Look into https://github.com/sbaeumlisberger/VirtualizingWrapPanel
- Look into better game banner resizing since Origins doesn't look great. Alternatively just manually edit the Origins banner to have different ratio?
- Add option to change app accent color
- Create new styles for combobox and textbox
- Redesign remaining parts of the app to use new flat styles, such as the About page and some dialogs
- Use new app icon by Nic? Twitter votes were very even.
- The option to disable animations currently only effects things like tab animations and not other transitions - improve? Or remove option altogether?
- Create a snackbar system for messages. Show for minor things too, like trying to open a window when already opened.
- Update the UI for selecting the app language. Have each language show a flag, a percentage for the translation completion and credits to the translators.
- Redesign the program selection dialog and improve scrolling

## 📦 Mod Loader
- Allow mods to specify an optional min app version
- Allow mods to add other mods as dependencies, specify them using mod ID and GameBanana mod ID
- Improve HTTP system for GameBanana mods. Have [retry policy](https://stackoverflow.com/questions/59702119/getting-response-status-code-does-not-indicate-success-502-bad-gateway-from) to avoid errors shown to the user. Ideally there also only be a single HttpClient in the app.
- Ideally the `files` module should have been two modules; `added_files` and `removed_files`. We could change it, but maybe that would just cause more confusion at this point?
- Optional grid view for downloadable mods so that more mods can be displayed at once
- Allow modding disc based games with a virtual file-system, primarily for Rayman 1 on PS1
- Add `costumes` module for Origins and Legends, allowing new costumes to be added, with the mod loader then merging them all together into the gameconfig to avoid file conflicts
- Make it more clear you can't install mods while mod loader is open (such as from game setup actions)
- Downloading big 1-click file causes RCP to freeze before it extracts it ([example](https://gamebanana.com/mods/490773))

### 🍌 GameBanana
- Link [Rayman 1](https://gamebanana.com/games/21305) to RCP
- Split [Rayman Jungle Run](https://gamebanana.com/games/19728) and [Rayman Fiesta Run](https://gamebanana.com/games/19729) between Microsoft Store and Ubisoft Connect versions since mods aren't compatible between versions

## 📦 Archive Explorer
- Add launch handlers for common archives like .ipk and .cnt so they can be opened in RCP directly from Windows File Explorer. Check if it's in a game folder to find version info, otherwise ask the user.
- Show warning message when replacing a file with one having a different file extensions (in `ReplaceFileAsync`) - the user should probably be using "import" then instead
- When importing or exporting files, have dialog for options (compression, format etc.) - this replaces the current global .gf file settings
- Add "save as" option, repacking the archive to a new file
- Allow setting double-click action for files - currently it opens it for editing as current file extension which is rarely useful. Perhaps have some priority order for how to open files (for example: .png -> .jpg -> binary).
- Multi-select folders
- Option to rename folder
- Drag-drop folders/files to move them
- Improve how file renaming is handled - currently it moves the file, which causes it to be re-compressed
- Option to only export textures (or rather only export specific file types?)
- Allow converting when adding new files (so you can easily add new textures in the game's format)
- Support more texture formats for UbiArt, such as PS4, Switch etc. Some resources:
    - https://github.com/KillzXGaming/Switch-Toolbox
    - https://github.com/FanTranslatorsInternational/Kuriimu2
- Allow exporting textures as DDS if it's DXT compressed and thus retaining the same DXT compression. Useful for converting textures between platforms without needing to re-compress them.
- When repacking, instead of creating the temp archive file in user temp folder we should create it in the same directory as the archive to avoid slowdown when moving the file if it's on a different drive
- Add tooltips for import/replace options to clarify what they do

## 🎮 Games

### New Games
- Add second [Rayman Designer demo](https://archive.org/details/ctib10_97)
- Add demos and prototypes for console versions
- Add Rayman Origins and Legends PC demos (don't allow download since they're easily available on Ubisoft Connect/Steam)
- Add Dutch version of Print Studio
- Add remaining console games (GCN, Wii, NDS, Xbox etc.)

### Changes
- Separate the Print Studio downloads for the different versions (language/year), allow it to run from disc
- Rename Rayman 3 GBA Prototype to "preview" now that there are actual leaked prototypes
- Show green text that game is running and gray out play button
- Rewrite how the game installers work and support Rayman 1 games. Allow installing directly from bin/cue files. Alternatively look into allowing the native installers to run. RibShark made a patch for them.
- Make Mapper panel for Rayman Designer maps, allow importing maps and viewing/editing properties for existing ones
- Allow the RayMap component to be more dynamic, changing the link based on the game version/region
- Move [Ray1Editor](https://github.com/RayCarrot/Ray1Editor) into RCP. Have it be a game panel for Rayman 1. Would make it easier to support.
- Add setup game actions for the Rayman 2 Beta
- Add setup game action for Rayman 2 which shows issue if DEP conflicts with the game since it can cause crashes (is there a way to check this though?)
- Add recommended setup game action for fan-games if there is an update available (don't auto-update, but link to latest version)

## 🎮 Game Clients & Emulators
- Add emulator options - for example launch mGBA in fullscreen with `-f` launch argument
- Add GOG Galaxy as a game client
- If selecting a custom emulator we can look for GBA saves as .sav files by default since that's the most common way of handling it
- Add mGBA to runtime modifications - might need to do memory search to find pointer due to its complexity (no static pointer)
- Allow launching through Steam even if it's not a native Steam game. The user might have added the game as a non-Steam game.
- Rewrite DOSBox config. Have all values needed to run well. Include scaler, output and joystick options. Add info for each on what they do. Also explain how the autoexecute commands work. Don't use 3-way checkboxes cause it's confusing. Instead have some better way to do it?

## ⚒️ Game Settings
- Have settings which replace a file, such as the controller fixes, apply it through a mod in the mod loader rather than just manually replacing the file
- Add button mapping to Rayman 3
- Add settings to GBA games, allow edit things like Rayman Hoodlums' Revenge debug mode, volume etc.
- Add language selection for Rayman Origins/Legends, [see post](https://raymanpc.com/forum/viewtopic.php?p=1453231#p1453231) for more info - the Steam exe can also easily be patched to check the Registry for the language like the Uplay version does
- Add Italian and German language options to Rayman Arena
- Add option to toggle rumble for M/Arena (would need to edit the save files for this)
- Extended Rabbids Go Home options to fix 1080p widescreen (use options config file rather than registry for this)
- Add more Rayman Raving Rabbids config values, like brightness (they are floats, but stored as integers)
- Set recommended cycles value to be higher for Rayman 1?

## 🏆 Progression
- Remove progression page and have it only be accessible from the game panels. Have an "open" button which opens it in a window with more details, editing options etc.
- Alternatively redesign the progression page to be more similar to the games page. Left navbar with games, shows progressbar for percentage but no info. Then when you click on a game it shows details on the right with convert options, save editors, backup etc.
- Update the code to use some generic IFileSystem abstraction to access saves, making it easier to work across different platforms. Allow getting metadata for files since we need it for some console to get attributes for files, like PS1 memory cards.
- Add save editors. Currently you have to manually edit the JSON which is confusing. Instead have each game have a UI for editing common fields.
- Add button `Convert and copy save to game...` which brings up selection or drop-down. Allow to select slot to copy, destination game and destination slot to overwrite (or add as a new slot if game supports that). This allows converting a save between platforms, for examples *Rayman 1 PC* -> *Rayman Advance GBA*.
- Show game in progression page if there is backup, but not installed? Otherwise you don't know you had a backup from when it was installed.
- Add progression support for: 
    - Rayman Edutainment games
    - Rayman 2 GBC
    - Rayman Raving Rabbids PS2 (same format as PC, but minigame ids seem different?)

## 🗒️ Miscellaneous
- Load all recent mods in news feed at once using Task.WhenAll()
- Only cache 10 most recent app news entries. Save checksum. News.json has checksum and list of json files with 10 news each.
- Allow converter to convert folder. Or maybe have a dialog for advanced file selection options which we can reuse? You can then set filters among other things.
- Allow converting game localization files to csv? Can be imported into Excel then which might give a nicer overview.
- Add converter for converting UBIArt textures
- Add option to create desktop shortcut for a game group, like "Rayman 2", which when opened gives you a window allowing you to select which one to launch
- Use a service like [Weblate](https://weblate.org/) for localization
- Add `folder settings` to the app settings where the user specifies the folder RCP uses for storing data, downloading games etc.
