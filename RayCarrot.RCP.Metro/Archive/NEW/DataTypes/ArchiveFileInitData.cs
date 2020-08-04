using System.Collections.Generic;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive file data when initializing a file
    /// </summary>
    public class ArchiveFileInitData
    {
        public ArchiveFileInitData(ImageSource thumbnail, IEnumerable<DuoGridItemViewModel> fileInfo)
        {
            Thumbnail = thumbnail;
            FileInfo = fileInfo;
        }

        public ImageSource Thumbnail { get; }

        public IEnumerable<DuoGridItemViewModel> FileInfo { get; }
    }
}