using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using BinarySerializer;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class Mod_R1_ViewModel : Mod_ProcessEditorViewModel
{
    #region Constructor

    public Mod_R1_ViewModel()
    {
        EditorFields = new ObservableCollection<EditorFieldViewModel>();

        BindingOperations.EnableCollectionSynchronization(EditorFields, Application.Current);
    }

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
    private Mod_R1_MemoryData? _memData;

    #endregion

    #region Protected Properties

    protected override long GameBaseOffset => 0x01D3A1A0; // TODO-UPDATE: Have DOSBox version selection. Support BizHawk.
    protected override bool IsGameBaseAPointer => true;

    #endregion

    #region Public Properties

    public ObservableCollection<EditorFieldViewModel> EditorFields { get; }

    #endregion

    #region Protected Methods

    protected override void InitializeFields(Pointer offset)
    {
        EditorFields.Clear();

        _memData = new Mod_R1_MemoryData(offset);

        EditorFields.Add(new EditorIntFieldViewModel(
            header: "Lives",
            info: null,
            getValueAction: () => _memData.StatusBar.LivesCount,
            setValueAction: x =>
            {
                _memData.StatusBar.LivesCount = (byte)x;
                _memData.PendingChange = true;
            },
            max: 99));
        EditorFields.Add(new EditorBoolFieldViewModel(
            header: "Place Ray",
            info: "Allows Rayman to be placed freely in the level",
            getValueAction: () => (short)_memData.RayMode < 0,
            setValueAction: x =>
            {
                bool isEnabled = (short)_memData.RayMode < 0;

                if (isEnabled != x)
                    _memData.RayMode = (RayMode)((short)_memData.RayMode * -1);

                _memData.PendingChange = true;
            }));
        EditorFields.Add(new EditorBoolFieldViewModel(
            header: "Power: Super-helico",
            info: null,
            getValueAction: () => (_memData.RayEvts & RayEvts.SuperHelico) != 0,
            setValueAction: x =>
            {
                if (x)
                    _memData.RayEvts |= RayEvts.SuperHelico;
                else
                    _memData.RayEvts &= ~RayEvts.SuperHelico;

                _memData.PendingChange = true;
            }));
    }

    protected override void ClearFields()
    {
        EditorFields.Clear();
        _memData = null;
    }

    protected override void RefreshFields()
    {
        // Serialize the data. Depending on if a value has changed
        // or not this will either read or write the data.
        _memData?.Serialize(Context);

        foreach (EditorFieldViewModel field in EditorFields)
            field.Refresh();
    }

    #endregion

    #region Public Methods

    public override void Dispose()
    {
        base.Dispose();

        _memData = null;

        BindingOperations.DisableCollectionSynchronization(EditorFields);
    }

    #endregion
}