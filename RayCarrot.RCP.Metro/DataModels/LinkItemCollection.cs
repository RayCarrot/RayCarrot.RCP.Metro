using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A collection of link item groups
    /// </summary>
    public class LinkItemCollection : ObservableCollection<LinkItemViewModel[]>
    {
        /// <summary>
        /// Adds a new group to the collection
        /// </summary>
        /// <param name="group">The group to add</param>
        public new void Add(LinkItemViewModel[] group)
        {
            if (group.Any(x => x.IsValid))
                base.Add(group);
        }
    }
}