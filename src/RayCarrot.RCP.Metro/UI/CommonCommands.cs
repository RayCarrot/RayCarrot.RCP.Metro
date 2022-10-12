using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

public static class CommonCommands
{
    private static ICommand? _copyToClipboardCommand;
    public static ICommand CopyToClipboardCommand => _copyToClipboardCommand ??= new RelayCommand(x =>
    {
        if (x == null)
            return;

        Clipboard.SetText(x.ToString());
    });
}