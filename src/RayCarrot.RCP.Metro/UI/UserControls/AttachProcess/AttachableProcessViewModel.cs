using System.Diagnostics;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

public class AttachableProcessViewModel : BaseViewModel, IDisposable
{
    public AttachableProcessViewModel(Process process, string filePath)
    {
        Process = process;
        FilePath = filePath;
        Icon16 = new BindableAsyncLazy<ImageSource>(() => LoadIconAsync(ShellThumbnailSize.Small));
        Icon32 = new BindableAsyncLazy<ImageSource>(() => LoadIconAsync(ShellThumbnailSize.Medium));

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
    
    public BindableAsyncLazy<ImageSource> Icon16 { get; }
    public BindableAsyncLazy<ImageSource> Icon32 { get; }

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

    public void Dispose()
    {
        Process.Dispose();
    }
}