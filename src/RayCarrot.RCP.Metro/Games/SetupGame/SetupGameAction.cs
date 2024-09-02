namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameAction
{
    public SetupGameAction(
        LocalizedString header, 
        LocalizedString info, 
        bool isComplete)
    {
        Header = header;
        Info = info;
        IsComplete = isComplete;
        FixActionIcon = GenericIconKind.None;
        FixActionDisplayName = null;
        FixAction = null;
    }

    public SetupGameAction(
        LocalizedString header, 
        LocalizedString info, 
        bool isComplete, 
        GenericIconKind fixActionIcon, 
        LocalizedString fixActionDisplayName, 
        Func<Task> fixAction)
    {
        Header = header;
        Info = info;
        IsComplete = isComplete;
        FixActionIcon = fixActionIcon;
        FixActionDisplayName = fixActionDisplayName;
        FixAction = fixAction;
    }

    public LocalizedString Header { get; }
    public LocalizedString Info { get; }
    
    public bool IsComplete { get; }

    public GenericIconKind FixActionIcon { get; }
    public LocalizedString? FixActionDisplayName { get; }
    public Func<Task>? FixAction { get; }
}