using System.Collections.Generic;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive file thumbnail data
    /// </summary>
    public class ArchiveFileThumbnailData
    {
        public ArchiveFileThumbnailData(ImageSource thumbnail, IEnumerable<DuoGridItemViewModel> fileInfo)
        {
            Thumbnail = thumbnail;
            FileInfo = fileInfo;
        }

        public ImageSource Thumbnail { get; }

        public IEnumerable<DuoGridItemViewModel> FileInfo { get; }
    }
}