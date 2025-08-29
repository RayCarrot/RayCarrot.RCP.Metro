using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

public static class CommonCommands
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static ICommand? _copyToClipboardCommand;
    public static ICommand CopyToClipboardCommand => _copyToClipboardCommand ??= new RelayCommand(x =>
    {
        if (x == null)
            return;

        try
        {
            Clipboard.SetText(x.ToString());
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Copying text to clipboard");
        }
    });
}