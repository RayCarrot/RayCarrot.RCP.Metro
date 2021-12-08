using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

public class ProgressionSlotViewModel : BaseViewModel
{
    public ProgressionSlotViewModel(LocalizedString? name, int index, int collectiblesCount, int totalCollectiblesCount, IEnumerable<ProgressionDataViewModel> dataItems)
    {
        Name = name ?? new ResourceLocString(nameof(Resources.Progression_GenericSlot), index + 1);
        Index = index;
        CollectiblesCount = collectiblesCount;
        TotalCollectiblesCount = totalCollectiblesCount;
        Percentage = collectiblesCount / (double)totalCollectiblesCount * 100;
        DataItems = new ObservableCollection<ProgressionDataViewModel>(dataItems);
        PrimaryDataItems = new ObservableCollection<ProgressionDataViewModel>(DataItems.Where(x => x.IsPrimaryItem));
        Is100Percent = CollectiblesCount == TotalCollectiblesCount;
    }

    public ProgressionSlotViewModel(LocalizedString? name, int index, double percentage, IEnumerable<ProgressionDataViewModel> dataItems)
    {
        Name = name ?? new ResourceLocString(Resources.Progression_GenericSlot, index + 1);
        Index = index;
        CollectiblesCount = null;
        TotalCollectiblesCount = null;
        Percentage = percentage;
        DataItems = new ObservableCollection<ProgressionDataViewModel>(dataItems);
        PrimaryDataItems = new ObservableCollection<ProgressionDataViewModel>(DataItems.Where(x => x.IsPrimaryItem));
        Is100Percent = percentage == 100;
    }

    public LocalizedString Name { get; }
    public int Index { get; }
    public int? CollectiblesCount { get; }
    public int? TotalCollectiblesCount { get; }
    public double Percentage { get; }
    public bool Is100Percent { get; }

    public Brush ProgressBrush
    {
        get
        {
            if (Is100Percent)
                return new SolidColorBrush(Color.FromRgb(76, 175, 80));
            else if (Percentage >= 50)
                return new SolidColorBrush(Color.FromRgb(33, 150, 243));
            else
                return new SolidColorBrush(Color.FromRgb(255, 87, 34));
        }
    }

    public ObservableCollection<ProgressionDataViewModel> PrimaryDataItems { get; }
    public ObservableCollection<ProgressionDataViewModel> DataItems { get; }
}