namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameActionsGroupSummaryViewModel : BaseViewModel
{
    public SetupGameActionsGroupSummaryViewModel(LocalizedString text, SetupGameActionState state)
    {
        Text = text;
        State = state;
    }

    public LocalizedString Text { get; }
    public SetupGameActionState State { get; }
}