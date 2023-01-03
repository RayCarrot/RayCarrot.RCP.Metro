using System.IO;
using System.Text;
using BinarySerializer;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class RaymanEdutainmentConfigViewModel : Ray1BaseConfigViewModel
{
    public RaymanEdutainmentConfigViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation) : 
        base(gameDescriptor, gameInstallation, Ray1EngineVersion.PC_Edu, LanguageMode.None)
    {
        PageSelection = new ObservableCollection<string>();
        RefreshSelection();
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void RefreshSelection()
    {
        var data = GameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);

        PageSelection.Clear();
        PageSelection.AddRange(data.AvailableGameModes);

        ResetSelectedPageSelectionIndex();

        Logger.Trace("EDU config selection has been modified with {0} items", PageSelection.Count);
    }

    public override string GetConfigFileName()
    {
        Logger.Debug("Getting DOS config file name using selected index {0}", SelectedPageSelectionIndex);

        if (SelectedPageSelectionIndex == -1)
            throw new Exception("Page selection is -1");

        string gameMode = PageSelection[SelectedPageSelectionIndex];

        Logger.Trace("Retrieving EDU config path for {0}", gameMode);

        // TODO-14: Don't find primary name like this. It's either EDU or QUI depending on game!
        // Get the primary name
        using FileStream stream = File.OpenRead(GameInstallation.InstallLocation + "PCMAP" + "COMMON.DAT");
        using Reader reader = new(stream);
        string primary = reader.ReadString(5, Encoding.UTF8);

        // Primary + secondary names
        return $"{primary}{gameMode}.CFG";
    }

    public override Task PageSelectionIndexChangedAsync()
    {
        Logger.Trace("EDU config selection changed to {0}", SelectedPageSelectionIndex);
        return LoadPageAsync();
    }

    /// <summary>
    /// Optional selection for the page
    /// </summary>
    public override ObservableCollection<string> PageSelection { get; }
}