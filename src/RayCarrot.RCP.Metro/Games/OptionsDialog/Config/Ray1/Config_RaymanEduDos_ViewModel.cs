using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;
using BinarySerializer.Ray1;
using NLog;

namespace RayCarrot.RCP.Metro;

public class Config_RaymanEduDos_ViewModel : Config_Ray1_BaseViewModel
{
    public Config_RaymanEduDos_ViewModel(GameInstallation gameInstallation) : 
        base(gameInstallation, Ray1EngineVersion.PC_Edu, LanguageMode.None)
    {
        PageSelection = new ObservableCollection<string>();
        RefreshSelection();
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void RefreshSelection()
    {
        PageSelection.Clear();
        PageSelection.AddRange(Data.Game_EducationalDosBoxGames.Select(x => $"{x.Name} ({x.LaunchMode})"));

        ResetSelectedPageSelectionIndex();

        Logger.Trace("EDU config selection has been modified with {0} items", PageSelection.Count);
    }

    public override string GetConfigFileName()
    {
        Logger.Debug("Getting DOS config file name using selected index {0}", SelectedPageSelectionIndex);

        if (SelectedPageSelectionIndex == -1)
            throw new Exception("Page selection is -1");

        var game = Data.Game_EducationalDosBoxGames[SelectedPageSelectionIndex];

        Logger.Trace("Retrieving EDU config path for '{0} ({1})'", game.Name, game.LaunchMode);

        // Get the primary name
        using FileStream stream = File.OpenRead(GameInstallation.InstallLocation + "PCMAP" + "COMMON.DAT");
        using Reader reader = new(stream);
        string primary = reader.ReadString(5, Encoding.UTF8);
        
        string? secondary = game.LaunchMode;

        return $"{primary}{secondary}.CFG";
    }

    public override Task PageSelectionIndexChangedAsync()
    {
        Logger.Trace("EDU config selection changed to {0}", SelectedPageSelectionIndex);
        return LoadPageAsync();
    }

    protected override Task OnGameInfoModified()
    {
        // Refresh the selection
        RefreshSelection();

        return LoadPageAsync();
    }

    /// <summary>
    /// Optional selection for the page
    /// </summary>
    public override ObservableCollection<string> PageSelection { get; }
}