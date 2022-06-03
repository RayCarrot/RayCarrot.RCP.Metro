using NLog;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class Mod_R1_ViewModel : Mod_BaseViewModel
{
    #region Constructor

    public Mod_R1_ViewModel()
    {
        // Test
        EditorFields = new ObservableCollection<EditorFieldViewModel>()
        {
            new EditorBoolFieldViewModel("A bool", "Some text", () => true, _ => { }),
            new EditorIntFieldViewModel("An int", null, () => 0, _ => { }),
            new EditorBoolFieldViewModel("A bool", "Some text", () => true, _ => { }),
            new EditorBoolFieldViewModel("A bool", "Some text", () => true, _ => { }),
            new EditorBoolFieldViewModel("A bool", "Some text", () => true, _ => { }),
            new EditorBoolFieldViewModel("A bool", "Some text", () => true, _ => { }),
            new EditorDropDownFieldViewModel("A drop-down", "Some stuff", () => 0, x => { }, () => new EditorDropDownFieldViewModel.DropDownItem[]
            {
                new("First option", null),
                new("Second option", null)
            })
        };
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands


    #endregion

    #region Mods Page

    public override GenericIconKind Icon => GenericIconKind.Games;
    public override LocalizedString Header => "Rayman 1"; // TODO-UPDATE: Localize
    public override object UIContent => _uiContent ??= new Mod_R1_UI()
    {
        DataContext = this
    };

    #endregion

    #region Private Fields

    private Mod_R1_UI? _uiContent;

    #endregion

    #region Public Properties

    public bool IsAttached { get; set; } // TODO: Create view model for attaching to game process
    public ObservableCollection<EditorFieldViewModel> EditorFields { get; }

    #endregion

    #region Public Methods

    public void RefreshFields()
    {
        foreach (EditorFieldViewModel field in EditorFields)
            field.Refresh();
    }

    public override Task InitializeAsync()
    {
        RefreshFields();
        return Task.CompletedTask;
    }

    #endregion
}