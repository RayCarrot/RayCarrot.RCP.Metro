using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.SimpleChildWindow;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    public class RCPChildWindow : ChildWindow
    {
        public RCPChildWindow()
        {
            MinimizeCommand = new RelayCommand(Minimize);
        }

        public static ObservableCollection<MinimizedChildWindow> MinimizedWindows { get; } = new ObservableCollection<MinimizedChildWindow>();

        public ICommand MinimizeCommand { get; }

        public bool IsMinimized
        {
            get => Visibility != Visibility.Visible;
            set => Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        }

        public void Minimize()
        {
            if (IsMinimized)
            {
                IsMinimized = false;
                MinimizedWindows.RemoveWhere(x => x.Window == this);
            }
            else
            {
                IsMinimized = true;
                MinimizedWindows.Add(new MinimizedChildWindow(this));
            }
        }

        // We need to wrap the windows in a class since we can't bind directly to the window due to how the logical tree works
        public record MinimizedChildWindow(RCPChildWindow Window);
    }
}