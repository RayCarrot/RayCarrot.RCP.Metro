using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer.Ray1;
using NLog;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro;

public class Config_RaymanEduDos_ViewModel : Config_Ray1_BaseViewModel
{
    public Config_RaymanEduDos_ViewModel(Games game) : base(game, Ray1EngineVersion.PC_Edu, LanguageMode.None)
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
        var game = Data.Game_EducationalDosBoxGames[SelectedPageSelectionIndex];

        Logger.Trace("Retrieving EDU config path for '{0} ({1})'", game.Name, game.LaunchMode);

        var installDir = Game.GetInstallDir();

        // Get the primary name
        string primary;

        using (var reader = File.OpenRead(installDir + "PCMAP" + "COMMON.DAT"))
        {
            var p = reader.Read(5);
            primary = Encoding.UTF8.GetString(p).TrimEnd('\0');
        }

        var secondary = game.LaunchMode;

        return $"{primary}{secondary}.CFG";
    }

    public override Task OnSelectedPageSelectionIndexUpdatedAsync()
    {
        Logger.Trace("EDU config selection changed");
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