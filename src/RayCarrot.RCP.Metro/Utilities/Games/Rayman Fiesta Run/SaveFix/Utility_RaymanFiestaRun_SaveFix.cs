﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Fiesta Run save fix utility
/// </summary>
public class Utility_RaymanFiestaRun_SaveFix : Utility<Utility_RaymanFiestaRun_SaveFix_UI, Utility_RaymanFiestaRun_SaveFix_ViewModel>
{
    public override string DisplayHeader => Resources.RFRU_SaveFixHeader;
    public override GenericIconKind Icon => GenericIconKind.Utilities_RaymanFiestaRun_SaveFix;
    public override string InfoText => Resources.RFRU_SaveFixInfo;
    public override bool IsAvailable => ViewModel.Editions.Any();
}