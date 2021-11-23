using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class GameOptionsDialog_ProgressionPageViewModel : GameOptionsDialog_BasePageViewModel
{
    #region Constructor

    public GameOptionsDialog_ProgressionPageViewModel(GameProgression_BaseViewModel progressionViewModel) 
        : base(new ResourceLocString(nameof(Resources.Progression_Header)), GenericIconKind.GameOptions_Progression)
    {
        ProgressionViewModel = progressionViewModel ?? throw new ArgumentNullException(nameof(progressionViewModel));
    }

    #endregion

    #region Public Properties

    public GameProgression_BaseViewModel ProgressionViewModel { get; }

    #endregion

    #region Protected Methods

    protected override object GetPageUI() => new GameProgression_UI()
    {
        DataContext = ProgressionViewModel
    };

    protected override Task LoadAsync()
    {
        return ProgressionViewModel.LoadDataAsync();
    }

    public override void Dispose()
    {
        // Dispose base
        base.Dispose();

        // Dispose progression data
        ProgressionViewModel?.Dispose();
    }

    #endregion
}