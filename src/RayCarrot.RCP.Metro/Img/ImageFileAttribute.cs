using System;

namespace RayCarrot.RCP.Metro;

// TODO-UPDATE: Add attribute to use on enum type which defines the folder the assets are stored in

/// <summary>
/// Defines the image file name associated with the field
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ImageFileAttribute : Attribute
{
    public ImageFileAttribute(string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; }
}