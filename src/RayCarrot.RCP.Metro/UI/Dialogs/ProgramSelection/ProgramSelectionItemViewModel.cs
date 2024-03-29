﻿using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

public class ProgramSelectionItemViewModel : BaseViewModel
{
    public ProgramSelectionItemViewModel(string name, FileSystemPath filePath)
    {
        Name = name;
        FilePath = filePath;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public string Name { get; }
    public ImageSource? IconImageSource { get; set; }
    public FileSystemPath FilePath { get; }
    public bool IsRecommended { get; init; }

    public void LoadIcon()
    {
        try
        {
            IconImageSource = WindowsHelpers.GetIconOrThumbnail(FilePath, ShellThumbnailSize.Medium).ToImageSource();
            IconImageSource?.Freeze();
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting program icon image source");
        }
    }
}