using System.Windows.Media;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// Archive file thumbnail data
/// </summary>
public record FileThumbnailData(ImageSource? Thumbnail, DuoGridItemViewModel[] FileInfo);