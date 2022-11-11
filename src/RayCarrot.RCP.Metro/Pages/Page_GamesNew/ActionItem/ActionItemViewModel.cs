namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a action item
/// </summary>
public abstract class ActionItemViewModel : BaseRCPViewModel
{
    protected ActionItemViewModel(UserLevel minUserLevel)
    {
        MinUserLevel = minUserLevel;
    }

    /// <summary>
    /// The minimum user level for the action
    /// </summary>
    public UserLevel MinUserLevel { get; }
}