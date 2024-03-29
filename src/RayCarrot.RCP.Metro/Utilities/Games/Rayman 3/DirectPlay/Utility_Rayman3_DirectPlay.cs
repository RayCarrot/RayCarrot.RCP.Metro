﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Direct Play utility
/// </summary>
public class Utility_Rayman3_DirectPlay : Utility<Utility_Rayman3_DirectPlay_Control, Utility_Rayman3_DirectPlay_ViewModel>
{
    public Utility_Rayman3_DirectPlay(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsLoadingDirectPlay))
                OnIsLoadingChanged();
        };
    }
    
    public GameInstallation GameInstallation { get; }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.R3U_DirectPlayHeader));
    public override GenericIconKind Icon => GenericIconKind.Utilities_Rayman3_DirectPlay;
    public override LocalizedString InfoText => new ResourceLocString(nameof(Resources.R3U_DirectPlayInfo));
    public override bool RequiresAdmin => true;

    public override bool IsLoading => ViewModel.IsLoadingDirectPlay;

    public override bool IsAvailable => AppViewModel.WindowsVersion is >= WindowsVersion.Win8 or WindowsVersion.Unknown;
    public override LocalizedString NotAvailableInfo => new ResourceLocString(nameof(Resources.R3U_DirectPlayNotAvailable));
}