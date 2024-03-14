using System.Diagnostics;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

public class ProcessViewModel : BaseViewModel
{
    public ProcessViewModel(Process process, ShellThumbnailSize iconSize)
    {
        Process = process;
        FilePath = process.MainModule!.FileName;
        Icon = new BindableAsyncLazy<ImageSource>(() => LoadIconAsync(iconSize));

        try
        {
            ProcessName = Process.ProcessName;
            WindowTitle = Process.MainWindowTitle;
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting process name and window title");

            ProcessName = String.Empty;
            WindowTitle = String.Empty;
        }
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public Process Process { get; }
    public string FilePath { get; }

    public string ProcessName { get; }
    public string WindowTitle { get; }
    
    public BindableAsyncLazy<ImageSource> Icon { get; }

    private Task<ImageSource?> LoadIconAsync(ShellThumbnailSize size)
    {
        return Task.Run(() =>
        {
            try
            {
                ImageSource? img = WindowsHelpers.GetIconOrThumbnail(FilePath, size).ToImageSource();
                img?.Freeze();
                return img;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Loading process icon from path");
                return null;
            }
        });
    }
}