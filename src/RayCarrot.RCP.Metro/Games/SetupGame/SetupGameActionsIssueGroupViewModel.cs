﻿namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameActionsIssueGroupViewModel : SetupGameActionsGroupViewModel
{
    public SetupGameActionsIssueGroupViewModel(GameInstallation gameInstallation, IEnumerable<SetupGameAction> actions)
        : base(gameInstallation, actions)
    {
        // TODO-LOC
        Header = "Issues";

        SummaryText = $"{TotalActions} issues"; // TODO-LOC: Singular/plural
        SummaryState = SetupGameActionState.Critical;
        ShowSummary = TotalActions > 0;
    }

    public override LocalizedString Header { get; }

    public override LocalizedString SummaryText { get; }
    public override SetupGameActionState SummaryState { get; }
    public override bool ShowSummary { get; }
}