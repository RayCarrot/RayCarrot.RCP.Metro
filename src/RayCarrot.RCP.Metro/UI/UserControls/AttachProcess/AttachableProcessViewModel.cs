using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using NLog;

namespace RayCarrot.RCP.Metro;

public class AttachableProcessViewModel : BaseViewModel, IDisposable
{
    public AttachableProcessViewModel(Process process, string filePath)
    {
        Process = process;
        FilePath = filePath;
        Icon16 = new BindableAsyncLazy<ImageSource>(() => LoadIconAsync(ShellThumbnailSize.Small));
        Icon32 = new BindableAsyncLazy<ImageSource>(() => LoadIconAsync(ShellThumbnailSize.Medium));
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public Process Process { get; }
    public string FilePath { get; }

    // TODO-UPDATE: Try/catch these? They might fail. Set from ctor.
    public string ProcessName => Process.ProcessName;
    public string WindowTitle => Process.MainWindowTitle;
    
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