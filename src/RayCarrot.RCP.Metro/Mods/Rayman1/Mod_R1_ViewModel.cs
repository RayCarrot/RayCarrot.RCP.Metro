using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using BinarySerializer;
using BinarySerializer.Ray1;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

public class Mod_R1_ViewModel : Mod_ProcessEditorViewModel<Mod_R1_MemoryData>
{
    #region Constructor

    public Mod_R1_ViewModel()
    {
        EditorFieldGroups = new ObservableCollection<EditorFieldGroupViewModel>();
        InfoItems = new ObservableCollection<DuoGridItemViewModel>();
        Actions = new ObservableCollection<Mod_ActionViewModel>();

        Emulators = new ObservableCollection<Mod_EmulatorViewModel>()
        {
            // TODO-UPDATE: Add 0.73-3 & BizHawk
            Mod_EmulatorViewModel.DOSBox_0_74,
        };
        SelectedEmulator = Emulators.First();
        ProcessNameKeywords = Emulators.SelectMany(x => x.ProcessNameKeywords).ToArray();

        GameVersions = new ObservableCollection<Mod_GameVersionViewModel>()
        {
            // TODO-UPDATE: Localize
            new("Rayman 1 (PC - 1.21)", () => Mod_R1_MemoryData.Offsets_PC_1_21),
        };
        SelectedGameVersion = GameVersions.First();

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

    #endregion

    #region Protected Properties

    protected override string[] ProcessNameKeywords { get; }
    protected override long GameBaseOffset => (SelectedEmulator ?? Emulators.First()).GameBaseOffset;
    protected override bool IsGameBaseAPointer => (SelectedEmulator ?? Emulators.First()).IsGameBaseAPointer;
    protected override Dictionary<string, long> Offsets => (SelectedGameVersion ?? GameVersions.First()).GetOffsetsFunc();

    #endregion

    #region Public Properties

    public ObservableCollection<Mod_EmulatorViewModel> Emulators { get; }
    public Mod_EmulatorViewModel? SelectedEmulator { get; set; }

    public ObservableCollection<Mod_GameVersionViewModel> GameVersions { get; }
    public Mod_GameVersionViewModel? SelectedGameVersion { get; set; }

    public ObservableCollection<EditorFieldGroupViewModel> EditorFieldGroups { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }
    public ObservableCollection<Mod_ActionViewModel> Actions { get; }

    #endregion

    #region Protected Methods

    protected override void InitializeContext(Context context)
    {
        base.InitializeContext(context);

        context.AddSettings(new Ray1Settings(Ray1EngineVersion.PC));
    }

    protected override void InitializeFields(Pointer offset)
    {
        base.InitializeFields(offset);

        EditorFieldGroups.Clear();
        InfoItems.Clear();
        Actions.Clear();

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
                new EditorIntFieldViewModel(
                    header: "Hit-points",
                    info: null,
                    getValueAction: () => AccessMemory(m => m.Ray?.HitPoints ?? 0),
                    setValueAction: x => AccessMemory(m =>
                    {
                        if (m.Ray == null)
                            return;

                        m.Ray.HitPoints = (byte)x; // TODO-UPDATE: Limit if don't have 5 max hp
                        m.PendingChange = true;
                    }),
                    max: 4),
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
                    header: "Map selection",
                    info: "Toggles the in-game map selection on the world map",
                    getValueAction: () => AccessMemory(m => m.AllWorld),
                    setValueAction: x => AccessMemory(m =>
                    {
                        m.AllWorld = x;
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

        InfoItems.Add(new DuoGridItemViewModel("Camera X", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.XMap}"))));
        InfoItems.Add(new DuoGridItemViewModel("Camera Y", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.YMap}"))));
        InfoItems.Add(new DuoGridItemViewModel("X position", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.Ray?.XPosition}"))));
        InfoItems.Add(new DuoGridItemViewModel("Y position", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.Ray?.YPosition}"))));
        InfoItems.Add(new DuoGridItemViewModel("Rayman state", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.Ray?.Etat}-{m.Ray?.SubEtat}"))));
        InfoItems.Add(new DuoGridItemViewModel("Helico time", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.HelicoTime}"))));
        InfoItems.Add(new DuoGridItemViewModel("Fist charge",
            new GeneratedLocString(() => AccessMemory(m => $"{m.Poing?.FistChargedLevel}"))));
        InfoItems.Add(new DuoGridItemViewModel("Active objects", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.ActiveObjCount}"))));
        InfoItems.Add(new DuoGridItemViewModel("Map time", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.MapTime}"))));
        InfoItems.Add(new DuoGridItemViewModel("Random index", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.RandomIndex}"))));
        InfoItems.Add(new DuoGridItemViewModel("Menu", 
            new GeneratedLocString(() => AccessMemory(m => $"{m.MenuEtape}"))));
        //InfoItems.Add(new DuoGridItemViewModel("Current map", 
        //    new GeneratedLocString(() => AccessMemory(m => $"{m.NumWorld}-{m.NumLevel}"))));
        //InfoItems.Add(new DuoGridItemViewModel("Selected map", 
        //    new GeneratedLocString(() => AccessMemory(m => $"{m.NumWorldChoice}-{m.NumLevelChoice}"))));

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
        Actions.Add(new Mod_ActionViewModel(
            header: "Return to world map", 
            iconKind: PackIconMaterialKind.MapOutline,
            command: new RelayCommand(() => AccessMemory(m =>
            {
                m.NewWorld = 1;
                m.PendingChange = true;
            })),
            isEnabledFunc: () => true));

        // Hack to fix a weird binding issue where the first int box gets set to 0
        AccessMemory(m =>
        {
            m.PendingChange = false;
            RefreshFields();
            m.PendingChange = false;
        });
    }

    protected override void ClearFields()
    {
        base.ClearFields();

        EditorFieldGroups.Clear();
        InfoItems.Clear();
        Actions.Clear();
    }

    protected override void RefreshFields()
    {
        base.RefreshFields();

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

        BindingOperations.DisableCollectionSynchronization(EditorFieldGroups);
        BindingOperations.DisableCollectionSynchronization(InfoItems);
    }
    
    #endregion
}