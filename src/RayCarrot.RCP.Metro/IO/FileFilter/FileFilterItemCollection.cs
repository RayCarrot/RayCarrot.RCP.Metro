using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A collection of items to use for filtering files when browsing
    /// </summary>
    public class FileFilterItemCollection : List<FileFilterItem>, IEqualityComparer<FileFilterItemCollection>
    {
        #region Constructors

        /// <summary>
        /// Constructor for an empty collection
        /// </summary>
        public FileFilterItemCollection() { }

        /// <summary>
        /// Constructor for using an existing collection
        /// </summary>
        /// <param name="items">The items to add to the collection</param>
        public FileFilterItemCollection(IEnumerable<FileFilterItem> items) : base(items) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Combines all filters into a single filter
        /// </summary>
        /// <returns>The filter containing all filters in the collection</returns>
        public FileFilterItem CombineAll(string description)
        {
            return new FileFilterItem(String.Join(";", this.Select(x => x.Filter)), description);
        }

        /// <summary>
        /// Add a new filter
        /// </summary>
        /// <param name="filter">The filter</param>
        /// <param name="description">The description of the filter</param>
        /// <exception cref="ArgumentException">An identical filter already exists</exception>
        public void Add(string filter, string description)
        {
            if (Exists(x => x.Filter != null && x.Filter.Equals(filter, StringComparison.CurrentCultureIgnoreCase)))
                throw new ArgumentException("An identical filter already exists");

            Add(new FileFilterItem(filter, description));
        }

        /// <summary>
        /// Determines whether the specified objects are equal
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(FileFilterItemCollection? x, FileFilterItemCollection? y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            if (x.Count != y.Count)
                return false;

            return !x.Where((t, i) => !t.Equals(y[i])).Any();
        }

        /// <summary>
        /// Returns a hash code for the specified object
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj">obj</paramref> is a reference type and <paramref name="obj">obj</paramref> is null.</exception>
        public int GetHashCode(FileFilterItemCollection obj)
        {
            return obj.Select(x => x.GetHashCode()).Sum();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is FileFilterItemCollection collection)
                return Equals(collection, this);
            else
                return false;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        /// <summary>
        /// Returns a string of the filters to be used when browsing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Count < 1)
                return String.Empty;
            else
                return String.Join("|", this);
        }

        #endregion
    }
}