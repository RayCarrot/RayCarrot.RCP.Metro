using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using BinarySerializer;
using BinarySerializer.Ray1;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

public class Mod_R1_ViewModel : Mod_ProcessEditorViewModel
{
    #region Constructor

    public Mod_R1_ViewModel()
    {
        EditorFieldGroups = new ObservableCollection<EditorFieldGroupViewModel>();
        InfoItems = new ObservableCollection<DuoGridItemViewModel>();
        Actions = new ObservableCollection<Mod_ActionViewModel>();

        BindingOperations.EnableCollectionSynchronization(EditorFieldGroups, Application.Current);
        BindingOperations.EnableCollectionSynchronization(InfoItems, Application.Current);
        BindingOperations.EnableCollectionSynchronization(Actions, Application.Current);
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
    private readonly object _lock = new();

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
    public ObservableCollection<Mod_ActionViewModel> Actions { get; }

    #endregion

    #region Private Methods

    private T? AccessMemory<T>(Func<Mod_R1_MemoryData, T> func)
    {
        if (_memData == null)
            return default;

        lock (_lock)
            return func(_memData);
    }

    private void AccessMemory(Action<Mod_R1_MemoryData> action)
    {
        if (_memData == null)
            return;

        lock (_lock)
            action(_memData);
    }

    #endregion

    #region Protected Methods

    protected override void InitializeContext(Context context)
    {
        context.AddSettings(new Ray1Settings(Ray1EngineVersion.PC));
    }

    protected override void InitializeFields(Pointer offset)
    {
        EditorFieldGroups.Clear();
        InfoItems.Clear();
        Actions.Clear();

        lock (_lock)
            _memData = new Mod_R1_MemoryData(offset);

        // TODO-UPDATE: Localize
        EditorFieldGroups.Add(new EditorFieldGroupViewModel(
            header: "General",
            editorFields: new EditorFieldViewModel[]
            {
                new EditorIntFieldViewModel(
                    header: "Lives",
                    info: null,
                    getValueAction: () => AccessMemory(m => m.StatusBar?.LivesCount ?? 0),
                    setValueAction: x => AccessMemory(m =>
                    {
                        if (m.StatusBar == null)
                            return;

                        m.StatusBar.LivesCount = (byte)x;
                        m.PendingChange = true;
                    }),
                    max: 99),
                new EditorIntFieldViewModel(
                    header: "Tings",
                    info: null,
                    getValueAction: () => AccessMemory(m => m.StatusBar?.TingsCount ?? 0),
                    setValueAction: x => AccessMemory(m =>
                    {
                        if (m.StatusBar == null)
                            return;

                        m.StatusBar.TingsCount = (byte)x;
                        m.PendingChange = true;
                    }),
                    max: 99),
                new EditorBoolFieldViewModel(
                    header: "5 hit points",
                    info: null,
                    getValueAction: () => AccessMemory(m => m.StatusBar?.MaxHealth == 4),
                    setValueAction: x => AccessMemory(m =>
                    {
                        if (m.StatusBar == null)
                            return;

                        m.StatusBar.MaxHealth = (byte)(x ? 4 : 2);
                        m.PendingChange = true;
                    })),
                new EditorBoolFieldViewModel(
                    header: "Place Ray",
                    info: "Allows Rayman to be placed freely in the level",
                    getValueAction: () => AccessMemory(m => (short)m.RayMode < 0),
                    setValueAction: x => AccessMemory(m =>
                    {
                        bool isEnabled = (short)m.RayMode < 0;

                        if (isEnabled != x)
                            m.RayMode = (RayMode)((short)m.RayMode * -1);

                        m.PendingChange = true;
                    })),
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
                getValueAction: () => AccessMemory(m => (m.RayEvts & ev.Evts) != 0),
                setValueAction: x => AccessMemory(m =>
                {
                    if (x)
                        m.RayEvts |= ev.Evts;
                    else
                        m.RayEvts &= ~ev.Evts;

                    m.PendingChange = true;
                }))).ToArray()));

        InfoItems.Add(new DuoGridItemViewModel("XMap", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.XMap}"))));
        InfoItems.Add(new DuoGridItemViewModel("YMap", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.YMap}"))));
        InfoItems.Add(new DuoGridItemViewModel("Map time", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.MapTime}"))));
        InfoItems.Add(new DuoGridItemViewModel("Fist charge",
            new GeneratedLocString(() => AccessMemory(m => $"{m.Poing?.FistChargedLevel}"))));

        Actions.Add(new Mod_ActionViewModel(
            header: "Finish level", 
            iconKind: PackIconMaterialKind.FlagOutline,
            command: new RelayCommand(() => AccessMemory(m =>
            {
                m.FinBoss = true;
                m.PendingChange = true;
            })), 
            isEnabledFunc: () => AccessMemory(m => m.Ray is { Etat: 0, SpeedX: 0, SpeedY: 0, Short_4A: -1 })));
        Actions.Add(new Mod_ActionViewModel(
            header: "Unlock all levels", 
            iconKind: PackIconMaterialKind.CircleMultipleOutline,
            command: new RelayCommand(() => AccessMemory(m =>
            {
                if (m.WorldInfo == null)
                    return;

                foreach (WorldInfo w in m.WorldInfo)
                {
                    if ((w.Runtime_State & 1) == 0)
                        w.Runtime_State |= 4;
                }

                m.PendingChange = true;
            })),
            isEnabledFunc: () => true));
        Actions.Add(new Mod_ActionViewModel(
            header: "All cages", 
            iconKind: PackIconMaterialKind.CircleMultiple,
            command: new RelayCommand(() => AccessMemory(m =>
            {
                if (m.WorldInfo == null)
                    return;

                foreach (WorldInfo w in m.WorldInfo)
                {
                    w.Runtime_Cages = 6;

                    if ((w.Runtime_State & 1) == 0)
                        w.Runtime_State |= 4;
                }

                m.PendingChange = true;
            })),
            isEnabledFunc: () => true));

        // Hack to fix a weird binding issue where the first int box gets set to 0
        _memData.PendingChange = false;
        RefreshFields();
        _memData.PendingChange = false;
    }

    protected override void ClearFields()
    {
        EditorFieldGroups.Clear();
        InfoItems.Clear();
        Actions.Clear();
        
        lock (_lock)
            _memData = null;
    }

    protected override void RefreshFields()
    {
        if (Context == null || _memData == null)
            return;

        lock (_lock)
        {
            // Serialize the data. Depending on if a value has changed
            // or not this will either read or write the data.
            _memData.Serialize(Context);
        }

        foreach (EditorFieldGroupViewModel group in EditorFieldGroups)
            group.Refresh();

        foreach (DuoGridItemViewModel item in InfoItems)
            if (item.Text is GeneratedLocString g)
                g.RefreshValue();

        foreach (Mod_ActionViewModel action in Actions)
            action.Refresh();
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