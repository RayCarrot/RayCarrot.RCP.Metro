namespace RayCarrot.RCP.Metro;

/// <summary>
/// Primarily used for view models to allow them to initialize and deinitialize
/// </summary>
public interface IInitializable
{
    void Initialize();
    void Deinitialize();
}