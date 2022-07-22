#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameOptions_EducationalDos_Control.xaml
/// </summary>
public partial class GameOptions_EducationalDos_Control : VMUserControl<GameOptions_EducationalDos_ViewModel>
{
    public GameOptions_EducationalDos_Control()
    {
        InitializeComponent();
        EducationalGameCollectionDropHandler.ViewModel = ViewModel;
    }
}