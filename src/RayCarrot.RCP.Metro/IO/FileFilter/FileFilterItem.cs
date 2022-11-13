using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// An item to use for filtering files when browsing
/// </summary>
public class FileFilterItem : IEqualityComparer<FileFilterItem>
{
    #region Constructors

    /// <summary>
    /// Default constructor
    /// </summary>
    public FileFilterItem()
    {

    }

    /// <summary>
    /// Constructor with filter arguments
    /// </summary>
    /// <param name="filter">The filter</param>
    /// <param name="description">The description of the filter</param>
    public FileFilterItem(string filter, string description)
    {
        Filter = filter;
        Description = description;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The filter
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// The description of the filter
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The string of the filter to be used when browsing
    /// </summary>
    public string StringRepresentation => ToString();

    #endregion

    #region Public Methods

    /// <summary>
    /// Determines whether the specified objects are equal
    /// </summary>
    /// <param name="x">The first object of type T to compare.</param>
    /// <param name="y">The second object of type T to compare.</param>
    /// <returns>true if the specified objects are equal; otherwise, false.</returns>
    public bool Equals(FileFilterItem? x, FileFilterItem? y)
    {
        return x?.StringRepresentation == y?.StringRepresentation;
    }

    /// <summary>
    /// Returns a hash code for the specified object
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj">obj</paramref> is a reference type and <paramref name="obj">obj</paramref> is null.</exception>
    public int GetHashCode(FileFilterItem obj)
    {
        return obj.StringRepresentation.GetHashCode();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is FileFilterItem item)
            return Equals(item, this);
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
    /// Returns a string of the filter to be used when browsing
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Description}({Filter}) | {Filter}";
    }

    #endregion
}