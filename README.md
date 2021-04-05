## Rayman Control Panel™ (Metro)
Rayman Control Panel™ is an open source community project created by RayCarrot. The purpose of this program is to unify existing game patches and fixes, as well as allowing extended configuration, for all PC Rayman games. This program does not come with any games and requires the user to have them installed. For certain games it is possible installing them through the program using a game disc.

Main features:
- Support for launching all installed Rayman PC games
- Extended game configuration support
- Game utilities, such as allowing per-level soundtrack in Rayman 1
- Save data backup/restore tool
- Disc installers to install select games from discs
- General utilities, such as the Archive Explorer to modify archive files (.cnt, .ipk etc.)

This repository is only for the WPF redesign. The WinForms version (version 1.0.0 - 3.2.2) repository has since been made private as it's no longer being maintained and is heavily outdated.

## Dependencies
The Rayman Control Panel uses these main dependencies:

- [RayCarrot.Logging](https://github.com/RayCarrot/RayCarrot.Logging)
- [RayCarrot.Common](https://github.com/RayCarrot/RayCarrot.Common)
- [RayCarrot.IO](https://github.com/RayCarrot/RayCarrot.IO)
- [RayCarrot.UI](https://github.com/RayCarrot/RayCarrot.UI)
- [RayCarrot.Binary](https://github.com/RayCarrot/RayCarrot.Binary)
- [RayCarrot.Windows.Shell](https://github.com/RayCarrot/RayCarrot.Windows.Shell)
- [RayCarrot.Windows.Registry](https://github.com/RayCarrot/RayCarrot.Windows.Registry)
- [RayCarrot.Rayman](https://github.com/RayCarrot/RayCarrot.Rayman)
- [RayCarrot.WPF](https://github.com/RayCarrot/Carrot-WPF)
- [Costura.Fody](https://github.com/Fody/Costura)
- [Infralution.Localization.Wpf](https://www.codeproject.com/Articles/35159/WPF-Localization-Using-RESX-Files)
- [DotNetZip](https://github.com/haf/DotNetZip.Semverd)
- [gong-wpf-dragdrop](https://github.com/punker76/gong-wpf-dragdrop)
- [Hardcodet.NotifyIcon.Wpf](http://www.hardcodet.net/wpf-notifyicon)
- [Magick.NET](https://github.com/dlemstra/Magick.NET)
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
- [MahApps.Metro.IconPacks.Material](https://github.com/MahApps/MahApps.Metro.IconPacks)
- [Microsoft.PowerShell.5.ReferenceAssemblies](https://www.nuget.org/packages/Microsoft.PowerShell.5.ReferenceAssemblies)
- [Microsoft.Windows.SDK.Contracts](https://www.nuget.org/packages/Microsoft.Windows.SDK.Contracts)
- [Microsoft.Xaml.Behaviors.Wpf](https://github.com/Microsoft/XamlBehaviorsWpf)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Nito.AsyncEx](https://github.com/StephenCleary/AsyncEx)
- [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged)
- [Resource.Embedder](https://github.com/MarcStan/Resource.Embedder)
- [XamlAnimatedGif](https://github.com/XamlAnimatedGif/XamlAnimatedGif)
- [AutoCompleteTextBox](https://github.com/quicoli/WPF-AutoComplete-TextBox)
- [WPFTextBoxAutoComplete](https://github.com/Nimgoble/WPFTextBoxAutoComplete)

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