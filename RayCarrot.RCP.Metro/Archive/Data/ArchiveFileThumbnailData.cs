using System.Collections.Generic;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive file thumbnail data
    /// </summary>
    public record ArchiveFileThumbnailData(ImageSource Thumbnail, IEnumerable<DuoGridItemViewModel> FileInfo);
}