namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameOptions_EducationalDos_UI.xaml
/// </summary>
public partial class GameOptions_EducationalDos_UI : VMUserControl<GameOptions_EducationalDos_ViewModel>
{
    public GameOptions_EducationalDos_UI()
    {
        InitializeComponent();
        EducationalGameCollectionDropHandler.ViewModel = ViewModel;
    }
}