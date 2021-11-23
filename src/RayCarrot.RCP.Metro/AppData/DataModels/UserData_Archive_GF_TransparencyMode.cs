namespace RayCarrot.RCP.Metro;

/// <summary>
/// The available transparency format modes when importing .gf files
/// </summary>
public enum UserData_Archive_GF_TransparencyMode
{
    /// <summary>
    /// Indicates that the format of the .gf file should always be preserved
    /// </summary>
    PreserveFormat,
        
    /// <summary>
    /// Indicates that the format should be updated based on the pixel format of the imported image
    /// </summary>
    UpdateBasedOnPixelFormat,

    /// <summary>
    /// Indicates that the format should be updated based on the pixel format of the imported image if it utilizes the alpha channel 
    /// </summary>
    UpdateBasedOnPixelFormatIfUtilized
}