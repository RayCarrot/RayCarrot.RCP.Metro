using RayCarrot.IO;
using RayCarrot.Rayman.Ray1;
using RayCarrot.UI;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayCarrot.Logging;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    public class Config_RaymanEduDos_ViewModel : Config_Ray1_BaseViewModel
    {
        public Config_RaymanEduDos_ViewModel(Games game) : base(game, Ray1Game.RayEdu, LanguageMode.None)
        {
            PageSelection = new ObservableCollection<string>();
            RefreshSelection();
        }
        
        public void RefreshSelection()
        {
            PageSelection.Clear();
            PageSelection.AddRange(Data.EducationalDosBoxGames.Select(x => $"{x.Name} ({x.LaunchMode})"));

            ResetSelectedPageSelectionIndex();

            RL.Logger?.LogTraceSource($"EDU config selection has been modified with {PageSelection.Count} items");
        }

        public override FileSystemPath GetConfigPath()
        {
            var game = Data.EducationalDosBoxGames[SelectedPageSelectionIndex];

            RL.Logger?.LogTraceSource($"Retrieving EDU config path for '{game.Name} ({game.LaunchMode})'");

            var installDir = Game.GetInstallDir();

            // Get the primary name
            string primary;

            using (var reader = File.OpenRead(installDir + "PCMAP" + "COMMON.DAT"))
            {
                var p = reader.Read(5);
                primary = Encoding.UTF8.GetString(p).TrimEnd('\0');
            }

            var secondary = game.LaunchMode;

            return Game.GetInstallDir() + $"{primary}{secondary}.CFG";
        }

        protected override Task OnSelectedPageSelectionIndexUpdatedAsync()
        {
            RL.Logger?.LogTraceSource($"EDU config selection changed");
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
}