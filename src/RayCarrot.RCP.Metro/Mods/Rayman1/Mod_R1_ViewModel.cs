using System.Collections.ObjectModel;
using System.Linq;
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
        EditorFieldGroups = new ObservableCollection<EditorFieldGroupViewModel>();
        InfoItems = new ObservableCollection<DuoGridItemViewModel>();

        BindingOperations.EnableCollectionSynchronization(EditorFieldGroups, Application.Current);
        BindingOperations.EnableCollectionSynchronization(InfoItems, Application.Current);
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

    protected override string[] ProcessNameKeywords => new[]
    {
        "DOSBox"
    };
    protected override long GameBaseOffset => 0x01D3A1A0; // TODO-UPDATE: Have DOSBox version selection. Support BizHawk.
    protected override bool IsGameBaseAPointer => true;

    #endregion

    #region Public Properties

    public ObservableCollection<EditorFieldGroupViewModel> EditorFieldGroups { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }

    #endregion

    #region Protected Methods

    protected override void InitializeFields(Pointer offset)
    {
        EditorFieldGroups.Clear();
        InfoItems.Clear();

        _memData = new Mod_R1_MemoryData(offset);

        // TODO-UPDATE: Localize
        EditorFieldGroups.Add(new EditorFieldGroupViewModel(
            header: "General",
            editorFields: new EditorFieldViewModel[]
            {
                new EditorIntFieldViewModel(
                    header: "Lives",
                    info: null,
                    getValueAction: () => _memData.StatusBar?.LivesCount ?? 0,
                    setValueAction: x =>
                    {
                        if (_memData.StatusBar == null)
                            return;

                        _memData.StatusBar.LivesCount = (byte)x;
                        _memData.PendingChange = true;
                    },
                    max: 99),
                new EditorIntFieldViewModel(
                    header: "Tings",
                    info: null,
                    getValueAction: () => _memData.StatusBar?.TingsCount ?? 0,
                    setValueAction: x =>
                    {
                        if (_memData.StatusBar == null)
                            return;

                        _memData.StatusBar.TingsCount = (byte)x;
                        _memData.PendingChange = true;
                    },
                    max: 99),
                new EditorBoolFieldViewModel(
                    header: "5 hit points",
                    info: null,
                    getValueAction: () => _memData.StatusBar?.MaxHealth == 4,
                    setValueAction: x =>
                    {
                        if (_memData.StatusBar == null)
                            return;

                        _memData.StatusBar.MaxHealth = (byte)(x ? 4 : 2);
                        _memData.PendingChange = true;
                    }),
                new EditorBoolFieldViewModel(
                    header: "Place Ray",
                    info: "Allows Rayman to be placed freely in the level",
                    getValueAction: () => (short)_memData.RayMode < 0,
                    setValueAction: x =>
                    {
                        bool isEnabled = (short)_memData.RayMode < 0;

                        if (isEnabled != x)
                            _memData.RayMode = (RayMode)((short)_memData.RayMode * -1);

                        _memData.PendingChange = true;
                    }),
            }));

        EditorFieldGroups.Add(new EditorFieldGroupViewModel(
            header: "Powers",
            editorFields: new (RayEvts Evts, LocalizedString Header)[]
            {
                (RayEvts.Fist, "Fist"),
                (RayEvts.Hang, "Hang"),
                (RayEvts.Helico, "Helico"),
                (RayEvts.Grab, "Grab"),
                (RayEvts.Run, "Run"),
                (RayEvts.Seed, "Seed"),
                (RayEvts.SuperHelico, "Super-helico"),
                (RayEvts.SquishedRayman, "Squished"),
                (RayEvts.Firefly, "Firefly"),
                (RayEvts.ForceRun, "Force run"),
                (RayEvts.ReverseControls, "Reverse controls"),
            }.Select(ev => new EditorBoolFieldViewModel(
                header: ev.Header,
                info: null,
                getValueAction: () => (_memData.RayEvts & ev.Evts) != 0,
                setValueAction: x =>
                {
                    if (x)
                        _memData.RayEvts |= ev.Evts;
                    else
                        _memData.RayEvts &= ~ev.Evts;

                    _memData.PendingChange = true;
                })).ToArray()));

        InfoItems.Add(new DuoGridItemViewModel("XMap", new GeneratedLocString(() => $"{_memData.XMap}")));
        InfoItems.Add(new DuoGridItemViewModel("YMap", new GeneratedLocString(() => $"{_memData.YMap}")));
        InfoItems.Add(new DuoGridItemViewModel("Map time", new GeneratedLocString(() => $"{_memData.MapTime}")));
        InfoItems.Add(new DuoGridItemViewModel("Fist charge", new GeneratedLocString(() => $"{_memData.Poing?.FistChargedLevel}")));
    }

    protected override void ClearFields()
    {
        EditorFieldGroups.Clear();
        InfoItems.Clear();
        _memData = null;
    }

    protected override void RefreshFields()
    {
        if (Context == null)
            return;

        // Serialize the data. Depending on if a value has changed
        // or not this will either read or write the data.
        _memData?.Serialize(Context);

        foreach (EditorFieldGroupViewModel group in EditorFieldGroups)
            group.Refresh();

        foreach (DuoGridItemViewModel item in InfoItems)
            if (item.Text is GeneratedLocString g)
                g.RefreshValue();
    }

    #endregion

    #region Public Methods

    public override void Dispose()
    {
        base.Dispose();

        _memData = null;

        BindingOperations.DisableCollectionSynchronization(EditorFieldGroups);
        BindingOperations.DisableCollectionSynchronization(InfoItems);
    }

    #endregion
}